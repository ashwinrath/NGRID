using System;
using System.Text;
using NGRID.Client;

namespace ConsumerSample
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create NGRIDClient object to connect to NGRID
            //Name of this application: ConsumerSample
            var ngridClient = new NGRIDClient("ConsumerSample");

            //Register to MessageReceived event to get messages.
            ngridClient.MessageReceived += NGRIDClient_MessageReceived;

            //Connect to NGRID server
            ngridClient.Connect();

            //Wait user to press enter to terminate application
            Console.WriteLine("hit enter to quit...");
            Console.ReadLine();

            //Disconnect from NGRID server
            ngridClient.Disconnect();
        }

        /// <summary>
        /// This method handles received messages from other applications via NGRID.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Message parameters</param>
        static void NGRIDClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            //Get message
            var messageText = Encoding.UTF8.GetString(e.Message.MessageData);

            //Process message
            Console.WriteLine();
            Console.WriteLine("msg received : " + messageText);
            Console.WriteLine("Orig App  : " + e.Message.SourceApplicationName);

            //Acknowledge that message is properly handled and processed. So, it will be deleted from queue.
            e.Message.Acknowledge();
        }
    }
}
