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
}
