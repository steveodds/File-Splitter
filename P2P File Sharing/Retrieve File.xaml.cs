using System;
using System.Windows;

namespace P2P_File_Sharing
{
    /// <summary>
    /// Interaction logic for Retrieve_File.xaml
    /// </summary>
    public partial class Retrieve_File : Window
    {
        public Retrieve_File()
        {
            InitializeComponent();
            this.Left = Application.Current.MainWindow.Left + Application.Current.MainWindow.ActualWidth;
            this.Top = Application.Current.MainWindow.Top;

            DisplaySavedFiles();
        }

        private void DisplaySavedFiles()
        {
            try
            {
                var savedFiles = DBController.LoadSavedFiles();
                foreach (var file in savedFiles)
                {
                    lbStoredFiles.Items.Add(file.FileName);
                }
            }
            catch (Exception ex)
            {
                StatusMessage.PostToActivityBox("Could not fetch stored files.", MessageType.ERROR);
                var logger = new StatusMessage();
                logger.Log($"Could not get stored files: {ex}");
                logger.Log($"Trace: {ex.StackTrace}");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //TODO Refactor
            var filename = lbStoredFiles.SelectedItem.ToString();
            if (filename != null)
            {
                var fileDetails = DBController.ReadEncryptedFileDetails(filename);
                var decryptFile = new FileEncryptor(fileDetails);
                if (!decryptFile.IsAlreadyEncrypted())
                {
                    StatusMessage.PostToActivityBox("Cannot decrypt file: The file was already decrypted", MessageType.ERROR);
                    throw new Exception("File is already decrypted");
                }
                decryptFile.FileDecrypt();
                //TODO Make sure DB is updated
            }
        }
    }
}
