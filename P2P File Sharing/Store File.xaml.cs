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

namespace P2P_File_Sharing
{
    /// <summary>
    /// Interaction logic for Store_File.xaml
    /// </summary>
    public partial class Store_File : Window
    {
        public string encFile, pickedFile, generatedHash, pickedFileExtension = null;
        public static bool encryptState = false;
        public Store_File()
        {
            InitializeComponent();
            this.Left = Application.Current.MainWindow.Left + Application.Current.MainWindow.ActualWidth;
            this.Top = Application.Current.MainWindow.Top;            
        }

        public bool filePicker()
        {
            postActivity("Picking file...", 1);
            bool success = false;
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Multiselect = false,
                Title = "Select a document for distribution",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbClickHint.Text = "Working...";
                pickedFile = openFileDialog.FileName;
                FileInfo fileInfo = new FileInfo(pickedFile);
                success = true;
                tbPickedFile.Text = fileInfo.Name + " is selected.";
                postActivity("Successfully picked file", 1);
            }
            else
            {
                postActivity("Error: Failed to pick file.", 0);
                success = false;
            }


            return success;
        }

        public bool fileHash()
        {
            bool success = false;
            try
            {
                
                postActivity("\nCalculating MD5 checksum...", 1);
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(pickedFile))
                    {
                        var hash = md5.ComputeHash(stream);
                        generatedHash = BitConverter.ToString(hash).Replace("-", "").ToLower();
                        postActivity("\tSuccessfully generated hash.", 1);
                        success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                postActivity("Error: " + ex.Message, 0); //change to log file
                throw ex;
            }
            

            return success;
        }

        private static void fileEncryptor(string inputFile, string outputFile, string skey)
        {
            try
            {
                using (RijndaelManaged aes = new RijndaelManaged())
                {
                    byte[] key = ASCIIEncoding.UTF8.GetBytes(skey);

                    
                    byte[] IV = ASCIIEncoding.UTF8.GetBytes(skey);

                    using (FileStream fsCrypt = new FileStream(outputFile, FileMode.Create))
                    {
                        using (ICryptoTransform encryptor = aes.CreateEncryptor(key, IV))
                        {
                            using (CryptoStream cs = new CryptoStream(fsCrypt, encryptor, CryptoStreamMode.Write))
                            {
                                using (FileStream fsIn = new FileStream(inputFile, FileMode.Open))
                                {
                                    int data;
                                    while ((data = fsIn.ReadByte()) != -1)
                                    {
                                        cs.WriteByte((byte)data);
                                    }
                                }
                            }
                        }
                    }
                }
                postActivity("Encrypted file.", 1);
                encryptState = true;
            }
            catch (Exception ex)
            {
                postActivity("Failed. " + ex.Message, 0);  //TODO Log file
            }
        }

        public void fileZipper()
        {
            try
            {
                int segmentsCreated;
                using (ZipFile zip = new ZipFile())
                {
                    FileInfo fileInfo = new FileInfo(pickedFile);
                    //zip.AlternateEncoding = ;  // utf-8
                    string fileName = fileInfo.FullName.Substring(0, pickedFile.Length - fileInfo.Name.Length);
                    zip.AddDirectory(fileName);
                    zip.Comment = "This zip was created at " + System.DateTime.Now.ToString("G");
                    zip.MaxOutputSegmentSize = 500 * 1024; // 100k segments
                    if (!Directory.Exists(String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ZippedTemp\\", fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length))))
                    {
                        Directory.CreateDirectory(String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ZippedTemp\\", fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length)));

                    }
                    zip.Save(String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ZippedTemp\\", fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length), "\\", fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length),".zip"));

                    segmentsCreated = zip.NumberOfSegmentsForMostRecentSave;
                    postActivity("File zipped, " + segmentsCreated + " files created. Waiting for distribution...", 1);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public int peerCheck()
        {
            int numberOfPeers = 0;


            return numberOfPeers;
        }

        public int ledgerVer()
        {
            int thisLedgerVer, currentLedgerVer, needsUpdate;
            needsUpdate = 0;


            return needsUpdate;
        }

        public int fileChunker()
        {
            int success = 0;


            return success;
        }

        public int clusterChunk()
        {
            int success = 0;


            return success;
        }

        public int clusterDistribute()
        {
            int success = 0;


            return success;
        }

        public void cleanup()
        {

        }

        public static void postActivity(string activityMessage, int postType)
        {
            if (postType == 1)
            {
                P2P_File_Sharing.MainWindow.mainAppInstance.tbAppActivity.Text += activityMessage;
            }
            else if (postType == 0)
            {
                P2P_File_Sharing.MainWindow.mainAppInstance.tbAppActivity.Text = "";
                P2P_File_Sharing.MainWindow.mainAppInstance.tbAppActivity.Foreground = new SolidColorBrush(Colors.Red);
                P2P_File_Sharing.MainWindow.mainAppInstance.tbAppActivity.Text = activityMessage;
            }
            else
            {
                P2P_File_Sharing.MainWindow.mainAppInstance.tbAppActivity.Text = "Failed to get message...";
            }
            
        }

        private void BtnSaveFileSAVE_Click(object sender, RoutedEventArgs e)
        {
            if (filePicker())
            {
                if (fileHash())
                {
                    string skey;
                    FileInfo fileInfo = new FileInfo(pickedFile);
                    skey = generatedHash.Substring(0, 16);
                    pickedFileExtension = System.IO.Path.GetExtension(pickedFile);
                    encFile = String.Concat(fileInfo.FullName.Substring(0, pickedFile.Length - 4), ".enc");
                    fileEncryptor(pickedFile, encFile, skey);
                }
                else
                {
                    postActivity("\nFailed to generate hash.", 0);
                }
            }

            if (encryptState)
            {
                encryptState = false;
                postActivity("Attempting to compress", 1);
                fileZipper();
            }
            else
            {
                postActivity("Process Failed: Encryption", 0);
            }
            
        }
    }
}
