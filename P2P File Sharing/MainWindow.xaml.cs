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
                //TODO Retrieve all stored files
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
