using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;
using NGRID.Communication.Events;
using NGRID.Communication.TCPCommunication;
using NGRID.Communication.Messages;
using NGRID.Exceptions;
using NGRID.Settings;
using NGRID.Threading;

namespace NGRID.Communication
{
    /// <summary>
    /// Represents communication layer of NGRID server. This class represents communicators as servers and applications
    /// to upper layers.
    /// </summary>
    public class CommunicationLayer : IRunnable
    {
        #region Private fields

        /// <summary>
        /// Reference to logger.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Reference to the settings.
        /// </summary>
        private readonly NGRIDSettings _settings;

        /// <summary>
        /// A collection that stores all remote applications.
        /// key: ApplicationId
        /// Total NGRIDRemoteApplication objects count is equal to
        /// (NGRIDClientApplication count + NGRIDAdjacentServer count + 1 NGRIDController).
        /// </summary>
        private readonly SortedList<int, NGRIDRemoteApplication> _remoteApplications;

        /// <summary>
        /// A collection that stores communication managers.
        /// </summary>
        private readonly List<ICommunicationManager> _communicationManagers;

        /// <summary>
        /// Temporary Communicator List. This list store a communicator until that communicator
        /// registers to NGRID server. After registration, it is removed from list.
        /// </summary>
        private readonly SortedList<long, ICommunicator> _communicators;

        /// <summary>
        /// Last generated application ID. This is used to get Unique ID for a RemoteApplication.
        /// It is used by CreateApplicationId method.
        /// </summary>
        private static int _lastApplicationId;

        /// <summary>
        /// Last generated communicator ID. When a new communicator builded, it gets 
        /// _lastCommunicatorId+1 by calling CreateCommunicatorId() method.
        /// </summary>
        private static long _lastCommunicatorId;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public CommunicationLayer()
        {
            _settings = NGRIDSettings.Instance;
            _remoteApplications = new SortedList<int, NGRIDRemoteApplication>();
            _communicators = new SortedList<long, ICommunicator>();
            _communicationManagers =
                new List<ICommunicationManager>
                    {
                        new TCPCommunicationManager(Convert.ToInt32(_settings["__ThisServerTCPPort"].Trim()))
                    };

            foreach (var manager in _communicationManagers)
            {
                manager.CommunicatorConnected += Manager_CommunicatorConnected;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Generates a Unique ID for Remote Applications.
        /// </summary>
        /// <returns>Unique ID</returns>
        public static int CreateApplicationId()
        {
            return Interlocked.Increment(ref _lastApplicationId);
        }

        /// <summary>
        /// Generates a Unique ID for communicators.
        /// </summary>
        /// <returns>Unique ID</returns>
        public static long CreateCommunicatorId()
        {
            return Interlocked.Increment(ref _lastCommunicatorId);
        }

        /// <summary>
        /// Starts the communication layer and all subsytems.
        /// </summary>
        public void Start()
        {
            foreach (var manager in _communicationManagers)
            {
                manager.Start();
            }
        }

        /// <summary>
        /// Stops the communication layer and all subsytems.
        /// </summary>
        /// <param name="waitToStop">Indicates that caller thread must wait
        /// until communication layer stops</param>
        public void Stop(bool waitToStop)
        {
            foreach (var manager in _communicationManagers)
            {
                manager.Stop(waitToStop);
            }

            StopCommunicators(waitToStop);
            ClearCommunicators(waitToStop);
        }

        /// <summary>
        /// Waits until communication layer stops.
        /// </summary>
        public void WaitToStop()
        {
            foreach (var manager in _communicationManagers)
            {
                manager.WaitToStop();
            }

            WaitToStopOfCommunicators();
            ClearCommunicators(true);
        }

        /// <summary>
        /// Adds a remote application to communication layer.
        /// </summary>
        /// <param name="application">Remote application to add</param>
        public void AddRemoteApplication(NGRIDRemoteApplication application)
        {
            lock (_remoteApplications)
            {
                if (!_remoteApplications.ContainsKey(application.ApplicationId))
                {
                    _remoteApplications.Add(application.ApplicationId, application);
                }
            }
        }

        /// <summary>
        /// Removes a remote application from communication layer.
        /// </summary>
        /// <param name="application">Remote application to remove</param>
        public void RemoveRemoteApplication(NGRIDRemoteApplication application)
        {
            lock (_remoteApplications)
            {
                if (_remoteApplications.ContainsKey(application.ApplicationId))
                {
                    if(application.ConnectedCommunicatorCount > 0)
                    {
                        throw new NGRIDException("Remote application can not be removed. It has " +
                                               application.ConnectedCommunicatorCount + " communicators connected.");
                    }

                    _remoteApplications.Remove(application.ApplicationId);
                }
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// When a communicator connects to server, this method is called.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        private void Manager_CommunicatorConnected(object sender, CommunicatorConnectedEventArgs e)
        {
            e.Communicator.StateChanged += Communicator_StateChanged;
            AddToCommunicators(e.Communicator);
            e.Communicator.MessageReceived += Communicator_MessageReceived;
            e.Communicator.Start();
        }

        #region Register message handling and processing methods

        /// <summary>
        /// When a message received from a communicator, this method is called.
        /// This method just process Register messages. After a register message received
        /// from cummunicator, stops listen to events from this communicator anymore.
        /// </summary>
        /// <param name="sender">Sender (ICommunicator)</param>
        /// <param name="e">Event args</param>
        private void Communicator_MessageReceived(object sender, MessageReceivedFromCommunicatorEventArgs e)
        {
            if (e.Message.MessageTypeId != NGRIDMessageFactory.MessageTypeIdNGRIDRegisterMessage)
            {
                return;
            }
            
            try
            {
                ProcessRegisterMessage(e.Communicator, e.Message as NGRIDRegisterMessage);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.Message, ex);
            }
            finally
            {
                e.Communicator.MessageReceived -= Communicator_MessageReceived;                
            }
        }

        /// <summary>
        /// Processes NGRIDRegisterMessage objects.
        /// </summary>
        /// <param name="communicator">Sender communicator of message</param>
        /// <param name="message">Message</param>
        private void ProcessRegisterMessage(ICommunicator communicator, NGRIDRegisterMessage message)
        {
            //Set the communicator properties
            communicator.CommunicationWay = message.CommunicationWay;

            NGRIDRemoteApplication remoteApplication = null;
            //Find remote application
            lock (_remoteApplications)
            {
                foreach (var app in _remoteApplications.Values)
                {
                    if (app.Name == message.Name && message.CommunicatorType == app.CommunicatorType)
                    {
                        remoteApplication = app;
                        break;
                    }
                }
            }

            //If application is found...
            if (remoteApplication != null)
            {
                try
                {
                    //Add communicator to communicator list of remote application
                    remoteApplication.AddCommunicator(communicator);
                    //Remove communicator from tempoary communicators list.
                    RemoveFromCommunicators(communicator.ComminicatorId);
                    //Send success message to remote application
                    SendOperationResultMessage(communicator, true, communicator.ComminicatorId.ToString(), message.MessageId);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex.Message, ex);
                    //An error occured, send failed message to remote application
                    SendOperationResultMessage(communicator, false, ex.Message, message.MessageId);
                    communicator.Stop(false);
                }
            }
            else //application == null
            {
                //Stop communicator, because a remote application can not connect this server that is not defined in settings file
                SendOperationResultMessage(communicator, false, "No remote application found with name: " + message.Name, message.MessageId);
                communicator.Stop(false);
            }
        }

        /// <summary>
        /// Sends a NGRIDOperationResultMessage message to a communicator.
        /// </summary>
        /// <param name="communicator">Communicator object</param>
        /// <param name="success">Operation result</param>
        /// <param name="resultText">Detailed result/error text</param>
        /// <param name="repliedMessageId">The message id of request message</param>
        private static void SendOperationResultMessage(ICommunicator communicator, bool success, string resultText, string repliedMessageId)
        {
            communicator.SendMessage(
                new NGRIDOperationResultMessage
                    {
                        Success = success,
                        ResultText = resultText,
                        RepliedMessageId = repliedMessageId
                    });
        }

        #endregion

        #region Communicator add/remove and stop methods

        /// <summary>
        /// When state of a communicator changes, this method handles event.
        /// It is used to remove a communicator from list when it is closed.
        /// </summary>
        /// <param name="sender">Sender (ICommunicationManager)</param>
        /// <param name="e">Event arguments</param>
        private void Communicator_StateChanged(object sender, CommunicatorStateChangedEventArgs e)
        {
            switch (e.Communicator.State)
            {
                case CommunicationStates.Closed:
                    RemoveFromCommunicators(e.Communicator.ComminicatorId);
                    break;
            }
        }

        /// <summary>
        /// Adds a TCPCommunicator object to _communicators list.
        /// </summary>
        /// <param name="communicator">TCPCommunicator to be added</param>
        private void AddToCommunicators(ICommunicator communicator)
        {
            lock (_communicators)
            {
                _communicators[communicator.ComminicatorId] = communicator;
            }
        }

        /// <summary>
        /// Removes a TCPCommunicator object from _communicators list.
        /// </summary>
        /// <param name="comminicatorId">Id of TCPCommunicator to be removed</param>
        private void RemoveFromCommunicators(long comminicatorId)
        {
            lock (_communicators)
            {
                if (_communicators.ContainsKey(comminicatorId))
                {
                    _communicators.Remove(comminicatorId);
                }
            }
        }

        /// <summary>
        /// Stops all communicator connections.
        /// </summary>
        /// <param name="waitToStop">Indicates that caller thread waits stopping of communicators</param>
        private void StopCommunicators(bool waitToStop)
        {
            lock (_communicators)
            {
                var communicatorIds = _communicators.Keys.ToArray();
                foreach (var communicatorId in communicatorIds)
                {
                    try
                    {
                        _communicators[communicatorId].Stop(waitToStop);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.Message, ex);
                    }
                }
            }
        }

        /// <summary>
        /// Removes all TCPCommunicator objects from _communicators list.
        /// </summary>
        /// <param name="waitToStop">Indicates that caller thread waits stop</param>
        private void ClearCommunicators(bool waitToStop)
        {
            if (!waitToStop)
            {
                return;
            }

            lock (_communicators)
            {
                _communicators.Clear();
            }
        }

        /// <summary>
        /// Waits all communicators to stop.
        /// </summary>
        private void WaitToStopOfCommunicators()
        {
            lock (_communicators)
            {
                var communicatorIds = _communicators.Keys.ToArray();
                foreach (var communicatorId in communicatorIds)
                {
                    try
                    {
                        _communicators[communicatorId].WaitToStop();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.Message, ex);
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}
