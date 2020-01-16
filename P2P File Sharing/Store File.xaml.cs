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
using System.Windows.Shapes;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.InteropServices;
using System.IO.Compression;
using Ionic.Zip;
using System.Data.SQLite;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace P2P_File_Sharing
{
    /// <summary>
    /// Interaction logic for Store_File.xaml
    /// </summary>
    public partial class Store_File : Window
    {
        private readonly string fClusteredTemp = String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ClusteredTemp");
        private readonly string fZippedTemp = String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ZippedTemp");
        private readonly string fPeer_Storage = String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage");

        public Store_File()
        {
            InitializeComponent();
            this.Left = Application.Current.MainWindow.Left + Application.Current.MainWindow.ActualWidth;
            this.Top = Application.Current.MainWindow.Top;
        }

        private void BtnSaveFileSAVE_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void TestMethod()
        {

        }
    }

    public class StoreFile
    {
        private string _fileName;
        private string _customLocation;

        public StoreFile(string fileName)
        {
            _fileName = fileName;
        }

        public StoreFile(string fileName, string customLocation)
        {
            _fileName = fileName;
            _customLocation = customLocation;
        }


        public static bool IsStored(string filename)
        {
            //TODO Call method in DB controller class to check if file exists
            return false;
        }

        private bool Store()
        {
            if (IsStored(_fileName))
                return false;

            //TODO Call method in DBController to store file in DB
            return false;
        }
    }
}
