using System;
using System.Linq;
using System.IO;
using System.Data.SQLite;
using System.Threading;
using System.Windows.Threading;
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
                    //TODO: Update status message to reflect these changes.
                    PostToActivityBox("Created storage folders.", MessageType.INFORMATION);
                }

                //TODO Send "Ready" message to status message.
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
            }

            public static bool IsDBFilePresent()
            {
                return File.Exists(_dbFile);
            }
            public static void DBCreate()
            {
                if (!IsDBFilePresent())
                {
                    SQLiteConnection.CreateFile("ds.sqlite");
                    DBBuilder();
                    DBCreate();
                }
                else
                {
                    object tempcheck;
                    long[] tablecheck = new long[5];
                    SQLiteConnection dsConTableCheck = new SQLiteConnection("Data Source=ds.sqlite;Version=3;");
                    dsConTableCheck.Open();

                    //TODO: Change information type stored in database
                    using (SQLiteCommand sQLiteCommand = new SQLiteCommand(dsConTableCheck))
                    {
                        sQLiteCommand.CommandText = "select count(*) from sqlite_master where type = 'table' and name = 'firstHash';";
                        tempcheck = sQLiteCommand.ExecuteScalar();
                        tablecheck[0] = (long)tempcheck;
                        sQLiteCommand.CommandText = "select count(*) from sqlite_master where type = 'table' and name = 'secondHash';";
                        tempcheck = sQLiteCommand.ExecuteScalar();
                        tablecheck[1] += (long)tempcheck;
                        sQLiteCommand.CommandText = "select count(*) from sqlite_master where type = 'table' and name = 'peerList';";
                        tempcheck = sQLiteCommand.ExecuteScalar();
                        tablecheck[2] += (long)tempcheck;
                        sQLiteCommand.CommandText = "select count(*) from sqlite_master where type = 'table' and name = 'dsFileLoc';";
                        tempcheck = sQLiteCommand.ExecuteScalar();
                        tablecheck[3] += (long)tempcheck;
                        sQLiteCommand.CommandText = "select count(*) from sqlite_master where type = 'table' and name = 'fileclusters';";
                        tempcheck = sQLiteCommand.ExecuteScalar();
                    }
                    tablecheck[4] += (long)tempcheck;

                    if (tablecheck.Sum() != 5)
                    {
                        DBBuilder();
                        DBCreate();
                    }
                }
            }

            private static void DBBuilder()
            {
                SQLiteConnection dsCon = new SQLiteConnection("Data Source=ds.sqlite;Version=3;");
                dsCon.Open();
                //TODO: Change database structure
                string createHashTable = "create table if not exists firstHash(filename varchar(256), hash varchar(50));";
                string createEncTable = "create table if not exists secondHash(filename varchar(256), encHash varchar(50));";
                string peerTable = "create table if not exists peerList(peerName varchar(50), peerLoc varchar(50));";
                string peerFiles = "create table if not exists dsFileLoc(clusterID varchar(50), peerLoc varchar(50));";
                string clusterCheck = "create table if not exists fileclusters(clusterID varchar(50), chunksNumber int);";

                SQLiteCommand creationCommands = new SQLiteCommand
                {
                    Connection = dsCon,
                };
                creationCommands.CommandText = createHashTable;
                creationCommands.CommandText += createEncTable;
                creationCommands.CommandText += peerTable;
                creationCommands.CommandText += peerFiles;
                creationCommands.CommandText += clusterCheck;

                creationCommands.ExecuteNonQuery();
            }
        }
    }
}
