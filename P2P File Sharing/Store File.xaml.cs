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
            postActivity("Picking file...", 0);
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
                isPostingActivityError(false);
                postActivity("Successfully picked file", 0);
            }
            else
            {
                isPostingActivityError(true);
                postActivity("Failed to pick file.", 0);
                success = false;
            }


            return success;
        }

        public bool fileHash()
        {
            bool success = false;
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

            return success;
        }

        private static void fileEncryptor(string inputFile, string outputFile, string skey)
        {
            try
            {
                using (RijndaelManaged aes = new RijndaelManaged())
                {
                    byte[] key = ASCIIEncoding.UTF8.GetBytes(skey);

                    /* This is for demostrating purposes only. 
                     * Ideally you will want the IV key to be different from your key and you should always generate a new one for each encryption in other to achieve maximum security*/
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
                postActivity("Encrypted file.", 0);
                encryptState = true;
            }
            catch (Exception ex)
            {
                isPostingActivityError(true);
                postActivity("Failed. " + ex.Message, 0);
            }
        }

        public void fileZipper()
        {
            try
            {
                FileInfo fi = new FileInfo(encFile);
                using (FileStream inFile = fi.OpenRead())
                {
                    // Prevent compressing hidden and 
                    // already compressed files.
                    if (fi.Extension != ".gz")
                    {
                        // Create the compressed file.
                        using (FileStream outFile = File.Create(fi.FullName + ".gz"))
                        {
                            using (GZipStream Compress = new GZipStream(outFile, CompressionMode.Compress))
                            {
                                // Copy the source file into 
                                // the compression stream.
                                inFile.CopyTo(Compress);

                                postActivity(String.Concat("Compressed ", fi.Name, " from ", fi.Length.ToString(), " to ", outFile.Length.ToString(), " bytes."), 0);
                            }
                        }
                    }
                    else
                    {
                        postActivity("Already compressed", 0);
                    }
                }
            }
            catch (Exception ex)
            {
                isPostingActivityError(true);
                postActivity("Failed to compress: " + ex.Message, 0);
                throw;
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

        public static void isPostingActivityError(bool status)
        {
            if (status)
            {
                postActivity("", 0);
                P2P_File_Sharing.MainWindow.mainAppInstance.tbAppActivity.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                P2P_File_Sharing.MainWindow.mainAppInstance.tbAppActivity.Foreground = new SolidColorBrush(Color.FromRgb(179, 179, 179));
            }
        }

        public static void postActivity(string activityMessage, int postType)
        {
            if (postType == 1)
            {
                P2P_File_Sharing.MainWindow.mainAppInstance.tbAppActivity.Text += activityMessage;
            }
            else
            {
                P2P_File_Sharing.MainWindow.mainAppInstance.tbAppActivity.Text = activityMessage;
            }
            
        }

        private void BtnSaveFileSAVE_Click(object sender, RoutedEventArgs e)
        {
            isPostingActivityError(false);
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
                    isPostingActivityError(true);
                    postActivity("\nFailed to generate hash.", 1);
                }
            }

            if (encryptState)
            {
                postActivity("Attempting to compress", 0);
                fileZipper();
            }
            
        }
    }
}
