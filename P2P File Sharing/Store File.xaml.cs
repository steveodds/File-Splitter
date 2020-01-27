using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace P2P_File_Sharing
{
    /// <summary>
    /// Interaction logic for Store_File.xaml
    /// </summary>
    public partial class Store_File : Window
    {
        public Store_File()
        {
            InitializeComponent();
            this.Left = Application.Current.MainWindow.Left + Application.Current.MainWindow.ActualWidth;
            this.Top = Application.Current.MainWindow.Top;
        }

        private void BtnSaveFileSAVE_Click(object sender, RoutedEventArgs e)
        {
            var filename = PickFile();
            var fileDetails = new FileInfo(filename);
            var fileObject = new EFile(fileDetails);
            var temp = DBController.ReadFileDetails(fileObject.FileLocation).FileName;
            if (fileObject.FileLocation != temp)
            {
                DBController.WriteToDB("files", fileObject);
                var encryptFile = new FileEncryptor(fileObject);
                try
                {
                    encryptFile.FileEncrypt();
                    fileObject.EncryptedHash = encryptFile.EncryptedHash;
                    fileObject.StoredDateTime = DateTime.Now;
                    DBController.WriteToDB("storedfiles", fileObject);
                    DBController.UpdateDBState(fileObject.FileHash, "true");
                    StatusMessage.PostToRecentsBox(fileObject);
                }
                catch (Exception ex)
                {
                    StatusMessage.PostToActivityBox("Attempting Encryption: " + ex.ToString(), MessageType.ERROR);
                    StatusMessage logger = new StatusMessage();
                    logger.Log("Save button: " + ex);
                }
            }
            else
            {
                MessageBox.Show("Error, file was already saved.", "Error encrypting file", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        private string PickFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Multiselect = false,
                Title = "Pick the file you want to store..."
            };
            openFileDialog.ShowDialog();

            return openFileDialog.FileName;
        }
    }
}
