using System.Windows;
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
            UpdateNoOfSavedFiles();
        }

        private void UpdateNoOfSavedFiles()
        {
            var noOfSavedFiles = DBController.LoadSavedFiles().Count;
            lblStoredFiles.Content = noOfSavedFiles;
        }

        private void BtnRetrieveFile_Click(object sender, RoutedEventArgs e)
        {
            Retrieve_File retrieve_File = new Retrieve_File();
            retrieve_File.ShowDialog();
            UpdateNoOfSavedFiles();
        }

        private void BtnRetrieveAll_Click(object sender, RoutedEventArgs e)
        {
            
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to retrieve all files?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                //TODO Retrieve all stored files
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Store_File store_File = new Store_File();
            PostToActivityBox("Pick a file for storage...", MessageType.NONE);
            store_File.ShowDialog();

            UpdateNoOfSavedFiles();
            PostToActivityBox("Ready.", MessageType.NONE);
        }
    }
}
