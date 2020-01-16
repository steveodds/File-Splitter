using System;
using System.IO;
using static P2P_File_Sharing.StatusMessage;

namespace P2P_File_Sharing
{
    public partial class MainWindow
    {
        public class StartupFunctions
        {
            private static readonly string _storageFolder = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\EFM-Storage";
            private static readonly string _encrypted = $@"{_storageFolder}\Encrypted";
            private static readonly string _compressed = $@"{_encrypted}\Compressed";
            private static readonly string _dbFile = $@"{Directory.GetCurrentDirectory()}\ds.sqlite";
            public string Encrypted
            {
                get
                {
                    return _encrypted;
                }
            }

            public string Compressed
            {
                get
                {
                    return _compressed;
                }
            }

            public static void DirectoryCheck()
            {
                if (!Directory.Exists(_storageFolder))
                {
                    Directory.CreateDirectory(_storageFolder);
                    Directory.CreateDirectory(_encrypted);
                    Directory.CreateDirectory(_compressed);
                    PostToActivityBox("Created storage folders.", MessageType.INFORMATION);
                }

                if (!Directory.Exists(_encrypted))
                {
                    Directory.CreateDirectory(_encrypted);
                    Directory.CreateDirectory(_compressed);
                    PostToActivityBox("Created missing folders.", MessageType.INFORMATION);
                }

                if (!Directory.Exists(_compressed))
                {
                    Directory.CreateDirectory(_compressed);
                    PostToActivityBox("Created missing folders.", MessageType.INFORMATION);
                }

                DBController.CreateDB();
            }
        }
    }
}
