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
    public partial class MainWindow : Window
    {
        public static MainWindow mainAppInstance;
        public MainWindow()
        {
            InitializeComponent();
            mainAppInstance = this;
            //new Thread(delegate () {directoryCheck();}).Start();
            Dispatcher.BeginInvoke(new Action(() => new Thread(delegate () { directoryCheck(); }).Start()), DispatcherPriority.ContextIdle, null);
            //directoryCheck();
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
                    Dispatcher.Invoke(new Action(() => { tbAppActivity.Text = ("Created required directory 'Peer_Storage' and other required directories in the Documents folder."); }), DispatcherPriority.ContextIdle);
                    Thread.Sleep(3000);
                    Dispatcher.Invoke(new Action(() => { tbAppActivity.Text = ("Ready"); }), DispatcherPriority.ContextIdle);
                }
            }
            else
            {
                Dispatcher.Invoke(new Action(() => { tbAppActivity.Text = ("Ready"); }), DispatcherPriority.ContextIdle);
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

        private void setActivityText(string message)
        {
            tbAppActivity.Text = message;
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
