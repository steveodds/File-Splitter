using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Data.SQLite;
using System.Net.Sockets;
using System.Collections;
using System.Net;
using System.Threading;

namespace P2P_File_Sharing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static ArrayList nSockets;
        public static MainWindow mainAppInstance;
        public MainWindow()
        {
            InitializeComponent();
            mainAppInstance = this;
            directoryCheck();
            dbCheck();
        }


        private void directoryCheck()
        {
            if (!Directory.Exists(String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage")))
            {
                Directory.CreateDirectory(String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage"));
                if (!Directory.Exists(String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ZippedTemp")) && !Directory.Exists(String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ClusteredTemp")) && !Directory.Exists(String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\Storage")))
                {
                    Directory.CreateDirectory(String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ZippedTemp"));
                    Directory.CreateDirectory(String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ClusteredTemp"));
                    Directory.CreateDirectory(String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\Storage"));
                    tbAppActivity.Text = "Created required directory 'Peer_Storage' and other required directories in the Documents folder.";
                }
            }
            else
            {
                tbAppActivity.Text = "Ready";
            }
        }

        private static void dbCheck()
        {
            if (!File.Exists(String.Concat(Directory.GetCurrentDirectory(),"\\ds.sqlite")))
            {
                SQLiteConnection.CreateFile("ds.sqlite");
                dbBuilder();
                dbCheck();
            }
            else
            {
                object tempcheck;
                long[] tablecheck = new long[5];
                SQLiteConnection dsConTableCheck = new SQLiteConnection("Data Source=ds.sqlite;Version=3;");
                dsConTableCheck.Open();

                SQLiteCommand sQLiteCommand = new SQLiteCommand(dsConTableCheck);
                sQLiteCommand.CommandText = "select count(*) from sqlite_master where type = 'table' and name = 'firstHash';";
                tempcheck = sQLiteCommand.ExecuteScalar();
                tablecheck[0] = (long)tempcheck;
                sQLiteCommand.CommandText = "select count(*) from sqlite_master where type = 'table' and name = 'secondHash';";
                tempcheck = sQLiteCommand.ExecuteScalar();
                tablecheck[1] += (long)tempcheck;
                sQLiteCommand.CommandText = "select count(*) from sqlite_master where type = 'table' and name = 'peerList';";
                tempcheck = sQLiteCommand.ExecuteScalar();
                tablecheck[2] += (long)tempcheck;
                sQLiteCommand.CommandText = "select count(*) from sqlite_master where type = 'table' and name = 'dsFileLoc';";
                tempcheck = sQLiteCommand.ExecuteScalar();
                tablecheck[3] += (long)tempcheck;
                sQLiteCommand.CommandText = "select count(*) from sqlite_master where type = 'table' and name = 'fileclusters';";
                tempcheck = sQLiteCommand.ExecuteScalar();
                tablecheck[4] += (long)tempcheck;

                if (tablecheck.Sum() != 5)
                {
                    dbBuilder();
                }
            }
        }

        protected static void dbBuilder()
        {
            SQLiteConnection dsCon = new SQLiteConnection("Data Source=ds.sqlite;Version=3;");
            dsCon.Open();
            string createHashTable = "create table if not exists firstHash(filename varchar(256), hash varchar(50));";
            string createEncTable = "create table if not exists secondHash(filename varchar(256), encHash varchar(50));";
            string peerTable = "create table if not exists peerList(peerName varchar(50), peerLoc varchar(50));";
            string peerFiles = "create table if not exists dsFileLoc(clusterID varchar(50), peerLoc varchar(50));";
            string clusterCheck = "create table if not exists fileclusters(clusterID varchar(50), chunksNumber int);";

            SQLiteCommand creationCommands = new SQLiteCommand();
            creationCommands.Connection = dsCon;
            creationCommands.CommandText = createHashTable;
            creationCommands.CommandText += createEncTable;
            creationCommands.CommandText += peerTable;
            creationCommands.CommandText += peerFiles;
            creationCommands.CommandText += clusterCheck;

            creationCommands.ExecuteNonQuery();
        }

        protected void sendFile(string filename, string receiverAddress)
        {
            //Stream fileStream = File.OpenRead(filename);
            //// Alocate memory space for the file
            //byte[] fileBuffer = new byte[fileStream.Length];
            //fileStream.Read(fileBuffer, 0, (int)fileStream.Length);
            //// Open a TCP/IP Connection and send the data
            //TcpClient clientSocket = new TcpClient(receiverAddress, 8080);
            //NetworkStream networkStream = clientSocket.GetStream();
            //networkStream.Write(fileBuffer, 0, fileBuffer.GetLength(0));
            //networkStream.Close();
        }

        //protected void conListener(string address)
        //{
        //    IPHostEntry IPHost = Dns.GetHostEntry(Dns.GetHostName());
        //    string ipAddress = IPHost.AddressList[0].ToString();
        //    nSockets = new ArrayList();
        //    Thread thdListener = new Thread(new ThreadStart(listenerThread));
        //    thdListener.Start();
        //}

        //private void listenerThread()
        //{
        //    string address;
        //    using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
        //    {
        //        socket.Connect("8.8.8.8", 65530);
        //        IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
        //        address = endPoint.Address.ToString();
        //    }
        //    IPAddress localAddr = IPAddress.Parse(address);
        //    TcpListener tcpListener = new TcpListener(localAddr, 8080);
        //    tcpListener.Start();
        //    while (true)
        //    {
        //        Socket handlerSocket = tcpListener.AcceptSocket();
        //        if (handlerSocket.Connected)
        //        {
        //            //Control.CheckForIllegalCrossThreadCalls = false;
        //            lock (this)
        //            {
        //                nSockets.Add(handlerSocket);
        //            }
        //            ThreadStart thdstHandler = new
        //            ThreadStart(handlerThread);
        //            Thread thdHandler = new Thread(thdstHandler);
        //            thdHandler.Start();
        //        }
        //    }
        //}

        //public void handlerThread()
        //{
        //    Socket handlerSocket = (Socket)nSockets[nSockets.Count - 1];
        //    NetworkStream networkStream = new NetworkStream(handlerSocket);
        //    int thisRead = 0;
        //    int blockSize = 1024;
        //    Byte[] dataByte = new Byte[blockSize];
        //    lock (this)
        //    {
        //        // Only one process can access
        //        // the same file at any given time
        //        Stream fileStream = File.OpenWrite("c:\\my documents\\SubmittedFile.txt");
        //        while (true)
        //        {
        //            thisRead = networkStream.Read(dataByte, 0, blockSize);
        //            fileStream.Write(dataByte, 0, thisRead);
        //            if (thisRead == 0) break;
        //        }
        //        fileStream.Close();
        //    }
        //    handlerSocket = null;
        //}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int storeFileWindowStatus;
            tbAppActivity.Text = "";
            Store_File store_File = new Store_File();
            store_File.ShowDialog();
            storeFileWindowStatus = store_File.closeStateM;
            if (storeFileWindowStatus == 1)
            {
                tbAppActivity.Text = "Ready";
            }
            else
            {
                tbAppActivity.Text = "Process 'Save File' cancelled, start again.";
            }
        }
    }
}
