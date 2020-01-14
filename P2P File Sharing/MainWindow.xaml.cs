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
using System.Windows.Threading;

namespace P2P_File_Sharing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// //test
    public partial class MainWindow : Window
    {
        Dispatcher serverdispatcher, directorydispatcher;
        bool file = false;
        string peerStorageF = String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage");
        string zippedTempF = String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ZippedTemp");
        string clusteredTemp = String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ClusteredTemp");
        string storageF = String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\Storage");
        public static MainWindow mainAppInstance;
        public string clientfileinfo = null;
        public MainWindow()
        {
            InitializeComponent();
            mainAppInstance = this;
            //new Thread(delegate () {directoryCheck();}).Start();
            directoryCheck();


            Dispatcher.BeginInvoke(new Action(() => new Thread(delegate () 
            {
                serverListenerThread();
            }).Start()), DispatcherPriority.ContextIdle, null);


            //directoryCheck();
            dbCheck();
            //AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
        }

        public void OnProcessExit(object sender, EventArgs e)
        {
            serverdispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
            directorydispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
            Application.Current.Shutdown();
        }

        private void directoryCheck()
        {
            directorydispatcher = Dispatcher.CurrentDispatcher;
            if (!Directory.Exists(peerStorageF))
            {
                Directory.CreateDirectory(peerStorageF);
                if (!Directory.Exists(zippedTempF) && !Directory.Exists(clusteredTemp) && !Directory.Exists(storageF))
                {
                    Directory.CreateDirectory(zippedTempF);
                    Directory.CreateDirectory(clusteredTemp);
                    Directory.CreateDirectory(storageF);
                    Dispatcher.Invoke(new Action(() => {
                        tbAppActivity.Text = ("Created required directory 'Peer_Storage' and other required directories in the Documents folder.");
                    }), DispatcherPriority.ContextIdle);
                    Thread.Sleep(3000);
                    Dispatcher.Invoke(new Action(() => {
                        tbAppActivity.Text = ("Ready");
                    }), DispatcherPriority.ContextIdle);
                }
            }
            else
            {
                Dispatcher.Invoke(new Action(() => {
                    tbAppActivity.Text = ("Ready");
                }), DispatcherPriority.ContextIdle);
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

                using (SQLiteCommand sQLiteCommand = new SQLiteCommand(dsConTableCheck))
                {
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
                }
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

        private void serverListenerThread()
        {
            serverdispatcher = Dispatcher.CurrentDispatcher;
            try
            {
                Socket sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sck.Bind(new IPEndPoint(IPAddress.Any, 1994));
                tbServerActivity.Text = "Listening...";
                sck.Listen(0);
                Socket acc = sck.Accept();
                tbServerActivity.Text = "Client connected.";

                string destination = storageF;
                string[] incomingDetails = new string[4];

                while (!file)
                {
                    byte[] msgbuffer = new byte[255];

                    int rec = acc.Receive(msgbuffer, 0, msgbuffer.Length, SocketFlags.None);
                    Array.Resize(ref msgbuffer, rec);
                    clientfileinfo = Encoding.Default.GetString(msgbuffer);
                    if (!clientfileinfo.Equals(null))
                    {
                        file = true;
                    }
                }
                tbServerActivity.Text = "Received file details";
                sck.Close();
                acc.Close();

                incomingDetails = clientfileinfo.Split(',');
                destination += "\\" + incomingDetails[1];
                Directory.CreateDirectory(destination);
                int numberOfFiles = int.Parse(incomingDetails[0]);
                string[] fileExts = new string[numberOfFiles];
                fileExts = incomingDetails[2].Split('-');

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Any, 1994));
                tbServerActivity.Text = "Waiting for files...";
                socket.Listen(0);
                Socket filesock = socket.Accept();
                tbServerActivity.Text = "Receiving files...";
                if (numberOfFiles > 0)
                {
                    for (int i = 0; i < numberOfFiles; i++)
                    {
                        using (Stream strm = new FileStream(destination + "." + fileExts[i], FileMode.CreateNew))
                        {
                            const int arSize = 100;
                            byte[] buffer = new byte[arSize];
                            SocketError errorCode;
                            int readBytes = -1;

                            int blockCtr = 0;
                            int totalReadBytes = 0;
                            while (readBytes != 0)
                            {
                                readBytes = filesock.Receive(buffer, 0, arSize, SocketFlags.None, out errorCode);
                                blockCtr++;
                                totalReadBytes += readBytes;
                                strm.Write(buffer, 0, readBytes);
                            }
                            strm.Close();
                        }
                    }
                    tbServerActivity.Text = "Done.";
                    socket.Close();
                    filesock.Close();
                }
                destination = storageF;
                serverListenerThread();
            }
            catch (Exception e)
            {
                //tbServerActivity.Text = e.ToString();
            }
        }

        private void setActivityText(string message)
        {
            tbAppActivity.Text = message;
        }

        private void BtnRetrieveFile_Click(object sender, RoutedEventArgs e)
        {
            Retrieve_File retrieve_File = new Retrieve_File();
            retrieve_File.ShowDialog();
        }

        private void BtnRetrieveAll_Click(object sender, RoutedEventArgs e)
        {
            
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to retrieve all files?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (messageBoxResult == MessageBoxResult.Yes)
            {

            }
        }

        private void BtnLeaveNetwork_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to leave the network?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (messageBoxResult == MessageBoxResult.Yes)
            {

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int storeFileWindowStatus;
            tbAppActivity.Text = "";
            Store_File store_File = new Store_File();
            store_File.ShowDialog();
            storeFileWindowStatus = store_File.closeStateM;
            if (storeFileWindowStatus == 1)
            {
                setActivityText("Ready");
            }
            else
            {
                setActivityText("Process 'Save File' cancelled.");
            }
        }
    }
}
