using System;
using System.Linq;
using System.IO;
using System.Data.SQLite;
using System.Threading;
using System.Windows.Threading;

namespace P2P_File_Sharing
{
    public partial class MainWindow
    {
        public class StartupFunctions
        {
            private static readonly string PEER_STORAGE_FOLDER = String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage");
            private static readonly string zippedTempF = String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ZippedTemp");
            private static readonly string clusteredTemp = String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\ClusteredTemp");
            private static readonly string storageF = String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage\\Storage");

            public static void DirectoryCheck()
            {
                if (!Directory.Exists(PEER_STORAGE_FOLDER))
                {
                    Directory.CreateDirectory(PEER_STORAGE_FOLDER);
                    if (!Directory.Exists(zippedTempF) && !Directory.Exists(clusteredTemp) && !Directory.Exists(storageF))
                    {
                        Directory.CreateDirectory(zippedTempF);
                        Directory.CreateDirectory(clusteredTemp);
                        Directory.CreateDirectory(storageF);
                        //TODO: Update status message to reflect these changes.
                    }
                }
                //TODO Send "Ready" message to status message.
            }

            public static void DBCheck()
            {
                if (!File.Exists(String.Concat(Directory.GetCurrentDirectory(), "\\ds.sqlite")))
                {
                    SQLiteConnection.CreateFile("ds.sqlite");
                    DBBuilder();
                    DBCheck();
                }
                else
                {
                    object tempcheck;
                    long[] tablecheck = new long[5];
                    SQLiteConnection dsConTableCheck = new SQLiteConnection("Data Source=ds.sqlite;Version=3;");
                    dsConTableCheck.Open();

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
                        DBCheck();
                    }
                }
            }

            private static void DBBuilder()
            {
                SQLiteConnection dsCon = new SQLiteConnection("Data Source=ds.sqlite;Version=3;");
                dsCon.Open();
                string createHashTable = "create table if not exists firstHash(filename varchar(256), hash varchar(50));";
                string createEncTable = "create table if not exists secondHash(filename varchar(256), encHash varchar(50));";
                string peerTable = "create table if not exists peerList(peerName varchar(50), peerLoc varchar(50));";
                string peerFiles = "create table if not exists dsFileLoc(clusterID varchar(50), peerLoc varchar(50));";
                string clusterCheck = "create table if not exists fileclusters(clusterID varchar(50), chunksNumber int);";

                SQLiteCommand creationCommands = new SQLiteCommand();
                creationCommands.Connection = dsCon;
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
