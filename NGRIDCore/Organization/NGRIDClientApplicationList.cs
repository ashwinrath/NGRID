using System;
using System.Collections.Generic;
using NGRID.Communication.WebServiceCommunication;
using NGRID.Exceptions;
using NGRID.Settings;
using NGRID.Threading;

namespace NGRID.Organization
{
    /// <summary>
    /// All Client applications that can send/receive messages to/from this NGRID server are stored in this class.
    /// </summary>
    public class NGRIDClientApplicationList : IRunnable
    {
        #region Public properties

        /// <summary>
        /// A collection that stores client applications.
        /// NGRIDClientApplication objects count is equals to total application definition in Settings file.
        /// </summary>
        public SortedList<string, NGRIDClientApplication> Applications { get; private set; }

        /// <summary>
        /// Reference to settings.
        /// </summary>
        private readonly NGRIDSettings _settings;

        #endregion

        #region Constrcutors

        /// <summary>
        /// Contructor. Gets applications from given settings file.
        /// </summary>
        public NGRIDClientApplicationList()
        {
            _settings = NGRIDSettings.Instance;
            Applications = new SortedList<string, NGRIDClientApplication>();
            try
            {
                CreateApplicationList();
            }
            catch (NGRIDException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new NGRIDException("Can not read settings file.", ex);
            }
        }

        #endregion

        #region Public methods

        public void Start()
        {
            foreach (var application in Applications.Values)
            {
                application.Start();
            }
        }

        public void Stop(bool waitToStop)
        {
            foreach (var application in Applications.Values)
            {
                application.Stop(waitToStop);
            }
        }

        public void WaitToStop()
        {
            foreach (var application in Applications.Values)
            {
                application.WaitToStop();
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Reads the xml file and creates client applications using _settings.
        /// </summary>
        private void CreateApplicationList()
        {
            foreach (var application in _settings.Applications)
            {
                //Create application object
                var clientApplication = new NGRIDClientApplication(application.Name);

                foreach (var channel in application.CommunicationChannels)
                {
                    switch (channel.CommunicationType)
                    {
                        case "WebService":
                            clientApplication.AddCommunicator(
                                new WebServiceCommunicator(
                                    channel.CommunicationSettings["Url"],
                                    Communication.CommunicationLayer.CreateCommunicatorId()
                                    ));
                            break;
                    }
                }

                //Add new application to list
                Applications.Add(clientApplication.Name, clientApplication);
            }
        }

        #endregion
    }
}
