using System;
using System.Collections.Generic;
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
using System.Net.Sockets;
using System.Collections;
using System.Net;
using System.Threading;
using System.Windows.Threading;
using static P2P_File_Sharing.StatusMessage;

namespace P2P_File_Sharing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow mainAppInstance;
        public string clientfileinfo = null;
        public MainWindow()
        {
            InitializeComponent();
            mainAppInstance = this;
            StartupFunctions.DirectoryCheck();
            StartupFunctions.DBCheck();

            //The below is server code that is no longer in use:
            //Dispatcher.BeginInvoke(new Action(() => new Thread(delegate () 
            //{
            //    serverListenerThread();
            //}).Start()), DispatcherPriority.ContextIdle, null);


            //AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
        }


        //private void serverListenerThread()
        //{
        //    serverdispatcher = Dispatcher.CurrentDispatcher;
        //    try
        //    {
        //        Socket sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //        sck.Bind(new IPEndPoint(IPAddress.Any, 1994));
        //        tbServerActivity.Text = "Listening...";
        //        sck.Listen(0);
        //        Socket acc = sck.Accept();
        //        tbServerActivity.Text = "Client connected.";

        //        string destination = storageF;
        //        string[] incomingDetails = new string[4];

        //        while (!file)
        //        {
        //            byte[] msgbuffer = new byte[255];

        //            int rec = acc.Receive(msgbuffer, 0, msgbuffer.Length, SocketFlags.None);
        //            Array.Resize(ref msgbuffer, rec);
        //            clientfileinfo = Encoding.Default.GetString(msgbuffer);
        //            if (!clientfileinfo.Equals(null))
        //            {
        //                file = true;
        //            }
        //        }
        //        tbServerActivity.Text = "Received file details";
        //        sck.Close();
        //        acc.Close();

        //        incomingDetails = clientfileinfo.Split(',');
        //        destination += "\\" + incomingDetails[1];
        //        Directory.CreateDirectory(destination);
        //        int numberOfFiles = int.Parse(incomingDetails[0]);
        //        string[] fileExts = new string[numberOfFiles];
        //        fileExts = incomingDetails[2].Split('-');

        //        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //        socket.Bind(new IPEndPoint(IPAddress.Any, 1994));
        //        tbServerActivity.Text = "Waiting for files...";
        //        socket.Listen(0);
        //        Socket filesock = socket.Accept();
        //        tbServerActivity.Text = "Receiving files...";
        //        if (numberOfFiles > 0)
        //        {
        //            for (int i = 0; i < numberOfFiles; i++)
        //            {
        //                using (Stream strm = new FileStream(destination + "." + fileExts[i], FileMode.CreateNew))
        //                {
        //                    const int arSize = 100;
        //                    byte[] buffer = new byte[arSize];
        //                    SocketError errorCode;
        //                    int readBytes = -1;

        //                    int blockCtr = 0;
        //                    int totalReadBytes = 0;
        //                    while (readBytes != 0)
        //                    {
        //                        readBytes = filesock.Receive(buffer, 0, arSize, SocketFlags.None, out errorCode);
        //                        blockCtr++;
        //                        totalReadBytes += readBytes;
        //                        strm.Write(buffer, 0, readBytes);
        //                    }
        //                    strm.Close();
        //                }
        //            }
        //            tbServerActivity.Text = "Done.";
        //            socket.Close();
        //            filesock.Close();
        //        }
        //        destination = storageF;
        //        serverListenerThread();
        //    }
        //    catch (Exception e)
        //    {
        //        //tbServerActivity.Text = e.ToString();
        //    }
        //}

        

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
                //TODO Retrieve all split files
            }
        }

        private void BtnLeaveNetwork_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to leave the network?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                //TODO Remove references to this feature
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Store_File store_File = new Store_File();
            PostToActivityBox("Pick a file for storage...", MessageType.NONE);
            store_File.ShowDialog();

            PostToActivityBox("Ready.", MessageType.NONE);
        }
    }
}
