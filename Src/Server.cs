using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.NetworkInformation;

namespace dbServer
{
    public class Server
	{
	
        public static int numberOfColumns;
        public static int maxThreads = 5;
        public static string[] labels;
        public static int secondLoopIndex;
        public static int firstLoopIndex;

        
        private static string SRSU_IP = GetIP.returnCheckCurrentIP();
        private static bool[] availableThread;
        private static bool directionsNecessary = true;
        private static string[] dataRecieveBuffer;

        public bool authorized = true;



        public Server() { }

        public static void init()
        {
            availableThread = new bool[maxThreads];
            dataRecieveBuffer = new string[maxThreads];

            for (int i = 0; i < maxThreads; i++)
                availableThread[i] = true;
        }

        public void startAServerThread()
        {
            if (availableThread == null)
                init();
            ThreadStart tstart = new ThreadStart(this.startServer);

            Thread serverThread = new Thread(tstart);
            serverThread.Start();
        }

        public static void loadData(int threadID_, string data)
        {
            dataRecieveBuffer[threadID_] = data;
        }

        private static bool dataRecieved(int threadID_)
        {
            if (dataRecieveBuffer[threadID_].Length > 0)
            {
                return true;
            }
            return false;
        }

        private static string getDataFromBuffer(int threadID_)
        {
            while (!dataRecieved(threadID_)) ;

            string value = dataRecieveBuffer[threadID_];
            dataRecieveBuffer[threadID_] = "";
            return value;
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

        private static void send(Socket socket, ASCIIEncoding asen, string message)
        {
            byte[] baAnswer = asen.GetBytes(message);
            socket.Send(baAnswer);
        }

        private static string getData(Socket socket, int size)
        {
            byte[] bb = new byte[size];
            int k = socket.Receive(bb);
            //.ToLower() fixes the blank column issues
            return toString(bb, k).ToLower();//added '.ToLower()'
            
        }

        private static int getMyThreadID()
        {
            for (int i = 0; i < availableThread.Length; i++)
            {
                if (availableThread[i])
                {
                    availableThread[i] = false;
                    return i;
                }
            }
            return -1;
        }

        private static void releaseThreadID(int threadID)
        {
            availableThread[threadID] = true;
        }

        static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 8002);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        public void startServer()
        {
            int myThreadID = getMyThreadID();

            try
            {

                IPAddress ipAd = IPAddress.Parse(SRSU_IP);
                // use local m/c IP address, and 
                // use the same in the client

                /* Initializes the Listener */
                int port = 8001 + myThreadID;
                TcpListener myList = new TcpListener(ipAd, FreeTcpPort());// TcpListener(ipAd, port)

                /* Start Listeneting at the specified port */
                myList.Start();

                Console.WriteLine("The server is running at port " + FreeTcpPort());// port
                Console.WriteLine("The local End point is  :" + myList.LocalEndpoint);

                Console.WriteLine("Waiting for a connection.....");
                Socket socket = myList.AcceptSocket();
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);//

                Console.WriteLine("Connection accepted from " + socket.RemoteEndPoint);

                this.startAServerThread();
                bool authorized = false;
                ASCIIEncoding asen = new ASCIIEncoding();

                while (true)
                {
                    try
                    {

                        if (!authorized)
                        {
                            Console.WriteLine("Recieved...");

                            string recieved = getData(socket, 256);
                           
                            if (recieved == "quit" || recieved == "exit") break;

                            
                            authorized = true;
                            if (!handleForCallCommand(socket, asen, myThreadID)) break;
                            if (!authorized)
                            {
                                send(socket, asen, "User not authorized...");
                            }
                            else if (!handleForCallCommand(socket, asen, myThreadID)) break;
                            

                        }
                        ///ALREADY LOGGED IN
                        else if (!handleForCallCommand(socket, asen, myThreadID)) break;
                        
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine("ERROR E1: " + e.ErrorCode);
                        break;
                    }
                }

                Console.WriteLine("Logged out...");
                socket.Close();
                releaseThreadID(myThreadID);

                myList.Stop();
            }
            catch (SocketException e)
            {
                // showing ERROR: 10048
                Console.WriteLine("ERROR E2: " + e.ErrorCode);
                releaseThreadID(myThreadID);
            }
        }

        public static string helpMenu() {
            string helpOptions = 
                "Menu Options:" + "\n" +
                "Enter 'quit' or 'exit' to close." + "\n" +
                "Enter 'new' to create a new table" + "\n" +
                "Enter 'show all' to show the list of tables" + "\n" +
                "Enter 'clear' to clear the screen" + "\n\n" +
                "Example Command:\nselect name from exampleTable";
            return helpOptions;
        }

        public static bool handleForCallCommand(Socket socket, ASCIIEncoding asen, int myThreadID)
        {

            if (directionsNecessary)
            {
                socket.Send(asen.GetBytes
                    ("Enter a command or 'help' to bring up the Help Menu.\ndb:> "));
                releaseThreadID(myThreadID);
                directionsNecessary = false;
            }
           
            string recievedCMD = getData(socket, 1024);
            Console.WriteLine(">>recievedCMD"+recievedCMD);

            if (recievedCMD == "quit") return false;


            else if (recievedCMD.ToLower() == "new")
            {
                send(socket, asen, "New Table Name\n:> ");
                string newTableName = getData(socket, 256);

                send(socket, asen, "\rHow many columns\n:> ");

                numberOfColumns = int.Parse(getData(socket, 256));

                labels = new string[numberOfColumns];

                for (int i = 0; i < numberOfColumns; i++)
                {
                    send(socket, asen, "\nColumn " + (i + 1) + "\n:> ");
                    string newLabel = getData(socket, 256);
                    labels[i] = newLabel;
                }

                send(socket, asen, "\rHow many rows in " + newTableName + "\n:> ");
                int numberOfRows = int.Parse(getData(socket, 256));

                string[] rows = new string[numberOfRows];
                for (firstLoopIndex = 0; firstLoopIndex < numberOfRows; firstLoopIndex++)
                {

                    string newRow = "";

                    for (secondLoopIndex = 0; secondLoopIndex < numberOfColumns; secondLoopIndex++)
                    {
                        send(socket, asen, "\rEnter  '" + labels[secondLoopIndex] + "' in "
                           + newTableName + " Row " + (firstLoopIndex + 1) + "\n:> ");

                        if (secondLoopIndex == numberOfColumns - 1)
                        {
                            string colm = getData(socket, 256);
                            newRow = newRow + colm;
                        }
                        else
                        {
                            string colm = getData(socket, 256) + '\t';//. /// puts space between elements under  labels
                            newRow = newRow + colm;
                        }
                    }
                    rows[firstLoopIndex] = newRow;

                }
                FileManager.willUpdateTables = true;
                FileManager.createTableM(newTableName, labels, rows);
                send(socket, asen, "clear screen");
                releaseThreadID(myThreadID);//keeps it from freezing after making the table
            }
            else if (recievedCMD.ToLower() == "help") {
                send(socket, asen, helpMenu() + "\n\nPress 'Enter' to exit help menu...\n");
                releaseThreadID(myThreadID);
            }
            else if (recievedCMD.ToLower() == "show all") {
                send(socket, asen, FileManager.getAllTables() + "\nEnd of Available Tables...");
                releaseThreadID(myThreadID);
            }
            else
            {
                try
                {
                    Console.WriteLine("Running Instruction = [" + myThreadID + ":" + recievedCMD + "]");
                    DBMS.doSQLCommand(myThreadID + "|" + recievedCMD);
                    Console.WriteLine("Instruction has been run!");
                    string fromDataBase = getDataFromBuffer(myThreadID);
                    socket.Send(asen.GetBytes(fromDataBase + "\nPress Enter to continue..."));
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR. wrong command " + e.StackTrace);
                    send(socket, asen, "Invalid Command...");
                    releaseThreadID(myThreadID);
                }
            }
            return true;
        }
    }
}

