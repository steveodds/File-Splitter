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
using System.Data.SQLite;


namespace P2P_File_Sharing
{
    /// <summary>
    /// Interaction logic for Store_File.xaml
    /// </summary>
    public partial class Store_File : Window
    {
        protected static string encFile, pickedFile, generatedHash, pickedFileExtension = null, thisDeviceID;
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
                pickedFile = openFileDialog.FileName;
                FileInfo fileInfo = new FileInfo(pickedFile);
                if (existsInDB(fileInfo.FullName))
                {
                    MessageBox.Show("This file has been processed before. Please pick another file.", "FILE EXISTS ON NETWORK", MessageBoxButton.OK, MessageBoxImage.Error);
                    postActivity("Error: Failed to pick file.", 0);
                }
                else
                {
                    success = true;
                    tbPickedFile.Text = fileInfo.Name + " is selected.";
                    postActivity("Successfully picked file", 1);
                }
            }
            else
            {
                postActivity("Error: Failed to pick file.", 0);
                success = false;
            }
            return success;
        }

        private static bool existsInDB(string filename)
        {
            bool existsinDB = false;
            long countNum = 0;
            object count;
            SQLiteConnection insertCon = new SQLiteConnection("Data Source=ds.sqlite;Version=3;");
            insertCon.Open();
            SQLiteCommand sQLiteCommand = new SQLiteCommand(insertCon);
            sQLiteCommand.CommandText = String.Format("select count(*) from firstHash where filename = @param1;");
            sQLiteCommand.Prepare();
            sQLiteCommand.Parameters.AddWithValue("@param1", filename);
            count = sQLiteCommand.ExecuteScalar();
            countNum = (long)count;
            if (countNum > 0)
            {
                existsinDB = true;
            }

            insertCon.Close();
            insertCon.Dispose();
            return existsinDB;
        }

        private static bool fileHash()
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
                        bool enterHash = insertToDB("firstHash", pickedFile, generatedHash);
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

        private static string fileHash(int enc)
        {
            string encHash = null;
            try
            {
                postActivity("\nCalculating MD5 checksum...", 1);
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(pickedFile))
                    {
                        var hash = md5.ComputeHash(stream);
                        encHash = BitConverter.ToString(hash).Replace("-", "").ToLower();
                    }
                }
            }
            catch (Exception ex)
            {
                postActivity("Error: " + ex.Message, 0); //change to log file
                throw ex;
            }


            return encHash;
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
                string encHash;
                encHash = fileHash(0);
                if (encHash != null)
                {
                    insertToDB("secondHash", inputFile, encHash);
                }
                postActivity("Encrypted file.", 1);
                encryptState = true;
            }
            catch (Exception ex)
            {
                postActivity("Failed. " + ex.Message, 0);  //TODO Log file
            }
        }

        private static int fileZipper()
        {
            int? segmentsCreated = null;
            try
            {
                using (ZipFile zip = new ZipFile())
                {
                    FileInfo zipFileInfo = new FileInfo(pickedFile);
                    //zip.AlternateEncoding = ;  // utf-8
                    string fileName = String.Concat(zipFileInfo.FullName.Substring(0, pickedFile.Length - zipFileInfo.Extension.Length), ".enc");
                    zip.AddFile(fileName);
                    zip.Comment = "This zip was created at " + System.DateTime.Now.ToString("G");
                    zip.MaxOutputSegmentSize = 500 * 1024; // 100k segments
                    if (!Directory.Exists(String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ZippedTemp\\", zipFileInfo.Name.Substring(0, zipFileInfo.Name.Length - zipFileInfo.Extension.Length))))
                    {
                        Directory.CreateDirectory(String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ZippedTemp\\", zipFileInfo.Name.Substring(0, zipFileInfo.Name.Length - zipFileInfo.Extension.Length)));

                    }
                    zip.Save(String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ZippedTemp\\", zipFileInfo.Name.Substring(0, zipFileInfo.Name.Length - zipFileInfo.Extension.Length), "\\", zipFileInfo.Name.Substring(0, zipFileInfo.Name.Length - zipFileInfo.Extension.Length),".zip"));

                    segmentsCreated = zip.NumberOfSegmentsForMostRecentSave;
                    postActivity("File zipped, " + segmentsCreated + " files created. Waiting for distribution...", 1);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return (int)segmentsCreated;
        }

        private static int fileClusterCalc(int segments, int peers)
        {
            int? clusterNum = null;
            int rem = segments % peers;

            if (rem > 0)
            {
                clusterNum = (segments / peers) + 1;
            }
            else
            {
                clusterNum = segments / peers;
            }
            return (int)clusterNum;
        }

        private static void clusterChunk(int segments)
        {
            int peers = 2, clusters = fileClusterCalc(segments, peers), numberOfFiles = 0, maxCluster = 0;
            string fileAsFolder, startfolder, destinationFolder, hostDir;
            FileInfo fileInfo = new FileInfo(pickedFile);
            fileAsFolder = fileInfo.Name.Substring(0, fileInfo.Name.Length - System.IO.Path.GetExtension(pickedFile).Length);
            startfolder = String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ZippedTemp\\", fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length));
            destinationFolder = String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ClusteredTemp\\", fileAsFolder, "\\", String.Concat(fileAsFolder, "_Cluster"));
            Directory.CreateDirectory(String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ClusteredTemp\\", fileAsFolder));
            hostDir = String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ClusteredTemp\\", fileAsFolder);
            for (int i = 0; i < peers; i++)
            {
                Directory.CreateDirectory(String.Concat(hostDir, "\\", String.Concat(fileAsFolder, "_Cluster", i + 1)));
            }
            numberOfFiles = Directory.GetFiles(String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ZippedTemp\\", fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length))).Length;
            string[] zippedFiles = new string[numberOfFiles];
            Array.Copy(Directory.GetFiles(String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ZippedTemp\\", fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length))), zippedFiles, numberOfFiles);
            Random random = new Random();
            maxCluster = peers;
            while (maxCluster > 0)
            {
                string[] clusterExts = new string[clusters];
                for (int i = 0; i < clusters; i++)
                {
                    bool fileExists = false;
                    string currentFile;
                    while (!fileExists)
                    {
                        if (Directory.EnumerateFileSystemEntries(startfolder).Any())
                        {
                            currentFile = zippedFiles[random.Next(numberOfFiles)];
                            fileExists = File.Exists(currentFile);
                            if (fileExists)
                            {
                                string ext = System.IO.Path.GetExtension(currentFile);
                                File.Move(currentFile, String.Concat(destinationFolder, maxCluster, "\\", generatedHash, ext));
                                clusterExts[i] = ext.Substring(1);
                            }
                        }
                        else
                        {
                            break;
                        }
                        
                    }
                      
                }
                string clusterHeader;
                clusterHeader = String.Concat("peer1\n", string.Join("\n", clusterExts));
                File.Create(String.Concat(destinationFolder, maxCluster, "\\_info")).Close();
                File.WriteAllText(String.Concat(destinationFolder, maxCluster, "\\_info"), clusterHeader);
                maxCluster--;
            }
            Directory.Delete(String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ZippedTemp\\", fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length)));
        }

        public void cleanup()
        {

        }

        private static string checkDB(string tablename, string column, string param1)
        {
            string fetchedString = null;


            return fetchedString;
        }

        private static bool insertToDB(string tablename, string param1, string param2)
        {
            bool insertResult = false;
            SQLiteConnection insertCon = new SQLiteConnection("Data Source=ds.sqlite;Version=3;");
            insertCon.Open();
            SQLiteCommand sQLiteCommand = new SQLiteCommand(insertCon);
            sQLiteCommand.CommandText = String.Format("insert into {0} values(@param1, @param2);", tablename);
            sQLiteCommand.Prepare();
            sQLiteCommand.Parameters.AddWithValue("@param1", param1);
            sQLiteCommand.Parameters.AddWithValue("@param2", param2);
            if (sQLiteCommand.ExecuteNonQuery() != 0)
            {
                insertResult = true;
            }
            return insertResult;
        }

        private static bool insertToDB(string tablename, string param1, int param2)
        {
            bool insertResult = false;
            SQLiteConnection insertCon = new SQLiteConnection("Data Source=ds.sqlite;Version=3;");
            insertCon.Open();
            SQLiteCommand sQLiteCommand = new SQLiteCommand(insertCon);
            sQLiteCommand.CommandText = String.Format("insert into {0} values('{1}','{2}');", tablename, param1, param2);
            if (sQLiteCommand.ExecuteNonQuery() != 0)
            {
                insertResult = true;
            }
            return insertResult;
        }
        public static void postActivity(string activityMessage)
        {
            P2P_File_Sharing.MainWindow.mainAppInstance.tbAppActivity.Text = "";
            P2P_File_Sharing.MainWindow.mainAppInstance.tbAppActivity.Text = activityMessage;
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
            tbClickHint.Text = "Working...";
            if (filePicker())
            {
                if (fileHash())
                {
                    string skey;
                    FileInfo fileInfo = new FileInfo(pickedFile);
                    skey = generatedHash.Substring(0, 16);
                    pickedFileExtension = System.IO.Path.GetExtension(pickedFile);
                    encFile = String.Concat(fileInfo.FullName.Substring(0, pickedFile.Length - System.IO.Path.GetExtension(pickedFile).Length), ".enc");
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
                int segments = fileZipper();
                clusterChunk(segments);
            }
            else
            {
                postActivity("Process Failed: Encryption", 0);
            }
            
        }
    }
}
