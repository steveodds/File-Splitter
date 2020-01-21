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
                var fileDetails = DBController.ReadFileDetails(filename);
                var decryptFile = new FileEncryptor(fileDetails);
                if (decryptFile.IsAlreadyDecrypted())
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
