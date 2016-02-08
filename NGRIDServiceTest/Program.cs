using System;
using NGRID;

namespace ServerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var server = new NGRIDServer();
                server.Start();

                Console.WriteLine("NGRID started.");
                Console.WriteLine("Press enter to stop...");
                Console.ReadLine();

                server.Stop(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
    }
}
