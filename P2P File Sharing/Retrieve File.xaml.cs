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

            lbStoredFiles.Items.Add("clientlist.doc");
            lbStoredFiles.Items.Add("widgets_android.txt");
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
