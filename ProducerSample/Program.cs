using System;
using System.Text;
using NGRID.Client;

namespace ProducerSample
{
    class Program
    {
        static void Main()
        {
            //Create NGRIDClient object to connect to NGRID
            //Name of this application: ProducerSample
            var ngridClient = new NGRIDClient("ProducerSample");

            //Connect to NGRID server
            ngridClient.Connect();

            Console.WriteLine("Enter text to be sent to consumer sample.");

            while (true)
            {
                //Get a message from user
                var messageText = Console.ReadLine();
                if (string.IsNullOrEmpty(messageText) || messageText == "exit")
                {
                    break;
                }

                //Create a NGRID Message to send to ConsumerSample
                var message = ngridClient.CreateMessage();
                //Set destination application name
                message.DestinationApplicationName = "ConsumerSample";
                //message.DestinationServerName = "someserver";
                //Set message data
                message.MessageData = Encoding.UTF8.GetBytes(messageText);

                //Send message
                message.Send();
            }

            //Disconnect from NGRID server
            ngridClient.Disconnect();
        }
    }
}
