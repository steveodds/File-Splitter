using System;
using System.Linq;
using System.IO;
using System.Data.SQLite;

namespace P2P_File_Sharing
{
    public class DBController
    {
        protected static readonly SQLiteConnection _dbCon = new SQLiteConnection("Data Source=ds.sqlite;Version=3;");
        private static readonly string _dbFile = $@"{Directory.GetCurrentDirectory()}\ds.sqlite";

        public DBController()
        {
            CreateDB();
        }

        public static bool Exists() => File.Exists(_dbFile);

        public static void CreateDB()
        {
            if (!Exists())
            {
                SQLiteConnection.CreateFile("Data Source=ds.sqlite;Version=3;");
                CreateTables();
            }

        }

        private static void CreateTables()
        {
            if (!HasTables())
            {
                SQLiteConnection dsCon = new SQLiteConnection("Data Source=ds.sqlite;Version=3;");
                dsCon.Open();
                string createFileTable = "create table if not exists files(filehash varchar(64), filename varchar(256), filestate boolean);";
                string createEncryptedFileTable = "create table if not exists storedfiles(filehash varchar(64), encryptedhash varchar(64), storedatetime varchar(25));";

                SQLiteCommand creationCommands = new SQLiteCommand
                {
                    Connection = dsCon,
                };
                creationCommands.CommandText = createFileTable;
                creationCommands.CommandText += createEncryptedFileTable;

                creationCommands.ExecuteNonQuery();
                dsCon.Close();
            }
        }

        private static bool HasTables()
        {
            object tempcheck;
            long[] tablecheck = new long[3];
            SQLiteConnection dsConTableCheck = new SQLiteConnection("Data Source=ds.sqlite;Version=3;");
            dsConTableCheck.Open();

            using (SQLiteCommand sQLiteCommand = new SQLiteCommand(dsConTableCheck))
            {
                sQLiteCommand.CommandText = "select count(*) from sqlite_master where type = 'table' and name = 'files';";
                tempcheck = sQLiteCommand.ExecuteScalar();
                tablecheck[0] = (long)tempcheck;
                sQLiteCommand.CommandText = "select count(*) from sqlite_master where type = 'table' and name = 'storedfiles';";
                tempcheck = sQLiteCommand.ExecuteScalar();
                tablecheck[1] += (long)tempcheck;
            }
            dsConTableCheck.Close();
            if (tablecheck.Sum() != 2)
                return false;

            return true;
        }

        public void WriteToDB(string tableName, string[] values)
        {
            _dbCon.Open();
            switch (tableName.ToLowerInvariant())
            {
                case "files":
                    using (SQLiteCommand sQLiteCommand = new SQLiteCommand(_dbCon))
                    {
                        //TODO Add insertion commands
                    }
                    break;

                case "storedfiles":
                    using (SQLiteCommand sQLiteCommand = new SQLiteCommand(_dbCon))
                    {
                        //TODO Add insertion commands
                    }
                    break;

                default:
                    _dbCon.Close();
                    throw new ArgumentException("The table does not exist");
            }
            _dbCon.Close();
        }

        public void ReadFromDB(string tablename, string column, string value)
        {
            //TODO Add code that reads from DB
        }
    }
}
