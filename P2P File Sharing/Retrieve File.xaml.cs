using System;
using System.Collections.Generic;
using System.Windows;

namespace P2P_File_Sharing
{
    /// <summary>
    /// Interaction logic for Retrieve_File.xaml
    /// </summary>
    public partial class Retrieve_File : Window
    {
        private List<EFile> savedFiles;
        private bool isListPopulated;

        public Retrieve_File()
        {
            InitializeComponent();
            this.Left = Application.Current.MainWindow.Left + Application.Current.MainWindow.ActualWidth;
            this.Top = Application.Current.MainWindow.Top;

            isListPopulated = ContainsSavedFiles();
            Loaded += Retrieve_File_Loaded;
        }

        private void Retrieve_File_Loaded(object sender, RoutedEventArgs e)
        {
            if (!isListPopulated)
            {
                this.Close();
            }
        }

        private bool ContainsSavedFiles()
        {
            lbStoredFiles.Items.Clear();
            try
            {
                savedFiles = DBController.LoadSavedFiles();
                if (savedFiles.Count > 0)
                {
                    foreach (var file in savedFiles)
                    {
                        lbStoredFiles.Items.Add(file.FileName);
                    }
                }
                else
                {
                    MessageBox.Show("There are no saved files.", "No Saved Files", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return false;
                }

            }
            catch (Exception ex)
            {
                StatusMessage.PostToActivityBox("Could not fetch stored files.", MessageType.ERROR);
                var logger = new StatusMessage();
                logger.Log($"Could not get stored files: {ex}");
                logger.Log($"Trace: {ex.StackTrace}");
            }
            return true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (lbStoredFiles.SelectedItems.Count > 0)
            {
                var filename = lbStoredFiles.SelectedItem.ToString();
                if (filename != null && savedFiles != null)
                {
                    EFile fileDetails = LoadData(filename);
                    fileDetails.FileName += ".aes";
                    var decryptFile = new FileEncryptor(fileDetails);
                    if (!decryptFile.IsAlreadyEncrypted())
                    {
                        MessageBox.Show("The file isn't encrypted.", "File Already Encrypted", MessageBoxButton.OK);
                        StatusMessage.PostToActivityBox("Cannot decrypt file: The file was already decrypted", MessageType.ERROR);
                    }

                    try
                    {
                        decryptFile.FileDecrypt();
                        DBController.UpdateDBState(fileDetails.FileHash, "false");
                        DBController.RemoveSavedFile(fileDetails.FileHash);
                        ContainsSavedFiles();
                        MessageBox.Show("File has been successfully retireved!", "File Retrieved", MessageBoxButton.OK, MessageBoxImage.Information);
                        StatusMessage.PostToActivityBox("File retrieved.", MessageType.INFORMATION);
                    }
                    catch (Exception decryptEx)
                    {
                        StatusMessage.PostToActivityBox("Failed to decrypt file!", MessageType.ERROR);
                        var logger = new StatusMessage();
                        logger.Log($"Retrieve File Button: {decryptEx}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a file to retrieve!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private EFile LoadData(string filename)
        {
            var file = savedFiles.Find(x => x.FileName.Contains(filename)).FileLocation;
            var fileDetails = DBController.ReadFileDetails(file);
            var genHash = new FileHash($"{file}.aes");
            var fullFileDetails = DBController.ReadEncryptedFileDetails(genHash.GenerateFileHash());
            fullFileDetails.FileLocation = fileDetails.FileLocation;
            fullFileDetails.FileName = fileDetails.FileName;
            fullFileDetails.IsStored = fileDetails.IsStored;
            return fullFileDetails;
        }
    }
}
