using System;
using System.Windows;

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
