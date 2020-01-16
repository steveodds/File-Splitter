using System;
using System.Linq;
using System.IO;
using System.Data.SQLite;
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
                    long[] tablecheck = new long[3];
                    SQLiteConnection dsConTableCheck = new SQLiteConnection("Data Source=ds.sqlite;Version=3;");
                    dsConTableCheck.Open();

                    //TODO: Change information type stored in database
                    using (SQLiteCommand sQLiteCommand = new SQLiteCommand(dsConTableCheck))
                    {
                        sQLiteCommand.CommandText = "select count(*) from sqlite_master where type = 'table' and name = 'files';";
                        tempcheck = sQLiteCommand.ExecuteScalar();
                        tablecheck[0] = (long)tempcheck;
                        sQLiteCommand.CommandText = "select count(*) from sqlite_master where type = 'table' and name = 'storedfiles';";
                        tempcheck = sQLiteCommand.ExecuteScalar();
                        tablecheck[1] += (long)tempcheck;
                    }
                    if (tablecheck.Sum() != 2)
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
                string createFileTable = "create table if not exists files(filehash varchar(64), filename varchar(256), filestate boolean);";
                string createEncryptedFileTable = "create table if not exists storedfiles(filehash varchar(64), encryptedhash varchar(64), storedatetime varchar(25));";

                SQLiteCommand creationCommands = new SQLiteCommand
                {
                    Connection = dsCon,
                };
                creationCommands.CommandText = createFileTable;
                creationCommands.CommandText += createEncryptedFileTable;

                creationCommands.ExecuteNonQuery();
            }
        }
    }
}
