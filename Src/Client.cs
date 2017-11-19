using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Net.Sockets;

namespace dbClient
{
    public class Client
    {
        private static string SRSU_IP = GetIP.returnCheckCurrentIP();

        public static void Main()
        {
            
            int port = 8001;

            while (!connect(port))
            {
                port++;
                Console.WriteLine("Trying port " + port);
                if (port > 8011) port = 8001;
            }
        }

        public static string toString(byte[] data, int size)
        {
            string recieved = "";
            for (int i = 0; i < size; i++)
            {
                recieved = recieved + Convert.ToChar(data[i]);
            }
            return recieved;
        }

        private static void send(Stream stream, ASCIIEncoding asen, string message)
        {

            byte[] baAnswer = asen.GetBytes(message);
            stream.Write(baAnswer, 0, baAnswer.Length);
        }

        private static string getData(Stream stream, int size)
        {
            byte[] bb = new byte[size];
            int k = stream.Read(bb, 0, size);
            return toString(bb, k);
        }


        private static string sendAndRecieve(Stream stream, ASCIIEncoding asen, string message, int size)
        {
            send(stream, asen, message);
            return getData(stream, size);
        }

        static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 8002);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        public static bool connect(int port)
        {
            try
            {
                Console.WriteLine("1. Connecting ...");
                TcpClient tcpclnt = new TcpClient();

                tcpclnt.Connect(SRSU_IP, FreeTcpPort());
                Stream stream = tcpclnt.GetStream();
                ASCIIEncoding asen = new ASCIIEncoding();

                Console.Clear();

                Console.Write(sendAndRecieve(stream, asen, "start", 256));

                while (true)
                {
                    string userResponse = Console.ReadLine();
                    if (userResponse.ToLower() == "quit" || userResponse.ToLower() == "exit")
                    {
                        send(stream, asen, userResponse.ToLower());
                        break;
                    }
                    else if (userResponse == "clear")
                    {
                        Console.Clear();
                        Console.WriteLine("Enter a command or 'help' to bring up the list of commands.");
                        Console.Write("db:> ");
                    }
                    //handles when the user just presses enter without entering anything
                    else if (userResponse == "")
                    {
                        Console.Write("db:> ");
                    }
                    else
                    {////////////////////////user issued command
                        string networkResponse = sendAndRecieve(stream, asen, userResponse, 1024);

                        if (networkResponse.StartsWith("clear"))
                        {
                            Console.Clear();
                            Console.WriteLine("Enter a command or 'help' to bring up the list of commands.");
                            Console.Write("db:> ");
                        }
                        //isn't normally called, but included just in case
                        else if (networkResponse == "")
                        {
                            Console.Write("db:> ");
                        }
                        else
                        {
                            Console.Write(networkResponse);
                        }
                    }
                }


                tcpclnt.Close();
                return true;
            }

            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
                return false;
            }
        }

    }
}