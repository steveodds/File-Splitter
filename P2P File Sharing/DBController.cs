using System;
using System.Linq;
using System.IO;
using System.Data.SQLite;
using System.Collections.Generic;

namespace P2P_File_Sharing
{
    public static class DBController
    {
        private static readonly SQLiteConnection _dbCon = new SQLiteConnection("Data Source=ds.sqlite;Version=3;");
        private static readonly string _dbFile = $@"{Directory.GetCurrentDirectory()}\ds.sqlite";



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

        internal static void UpdateDBState(string fileHash, string newState)
        {
            _dbCon.Open();
            try
            {
                using (var command = new SQLiteCommand(_dbCon))
                {
                    command.CommandText = "UPDATE files SET filestate=@state WHERE filehash=@filehash";
                    command.Parameters.AddWithValue("@state", newState);
                    command.Parameters.AddWithValue("@filehash", fileHash);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception dbUpdateEx)
            {
                StatusMessage.PostToActivityBox("Failed to update file save state.", MessageType.ERROR);
                StatusMessage status = new StatusMessage();
                status.Log($"Failed to write file state to file table: {dbUpdateEx}");
            }
            finally
            {
                _dbCon.Close();
            }
            _dbCon.Close();
        }

        internal static void RemoveSavedFile(string hash)
        {
            _dbCon.Open();
            try
            {
                using (SQLiteCommand command = new SQLiteCommand(_dbCon))
                {
                    command.CommandText = "DELETE FROM storedfiles WHERE filehash=@hash";
                    command.Parameters.AddWithValue("@hash", hash);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception deleteEx)
            {
                StatusMessage.PostToActivityBox("Failed to delete retrieved file record.", MessageType.ERROR);
                var logger = new StatusMessage();
                logger.Log($"DBController.RemoveSavedFile: Could not delete saved file record: {deleteEx}");
            }
            finally
            {
                _dbCon.Close();
            }
            _dbCon.Close();
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

        public static void WriteToDB(string tableName, EFile fileDetails)
        {
            SQLiteConnection dbConIN = new SQLiteConnection("Data Source=ds.sqlite;Version=3;");
            dbConIN.Open();
            switch (tableName.ToLowerInvariant())
            {
                case "files":
                    WriteToTables(tableName, fileDetails.ToStringArray());
                    break;

                case "storedfiles":
                    WriteToTables(tableName, fileDetails.ToStringArrayEncrypted());
                    break;

                default:
                    dbConIN.Close();
                    throw new ArgumentException("The table does not exist");
            }
            dbConIN.Close();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Pending>")]
        private static void WriteToTables(string table, string[] parameters)
        {
            if (string.IsNullOrEmpty(table))
                throw new ArgumentNullException("DB commands: No table name was given.");
            if (parameters.Length < 3)
                throw new ArgumentNullException("DB commands: Not enough arguments were given.");

            _dbCon.Open();
            try
            {
                using (SQLiteCommand sQLiteCommand = new SQLiteCommand(_dbCon))
                {
                    sQLiteCommand.CommandText =
                        $@"
                        INSERT INTO {table} 
                        VALUES ($param1, $param2, $param3)
                    ";
                    sQLiteCommand.Parameters.AddWithValue("$param1", parameters[0]);
                    sQLiteCommand.Parameters.AddWithValue("$param2", parameters[1]);
                    sQLiteCommand.Parameters.AddWithValue("$param3", parameters[2]);
                    sQLiteCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                StatusMessage.PostToActivityBox($"Writing to {table} table: " + ex, MessageType.ERROR);
                StatusMessage logger = new StatusMessage();
                logger.Log($"DBController.WriteToTables with {table} as the table and {parameters.Length} parameters. Exception: \n" + ex);
            }
            finally
            {
                _dbCon.Close();
            }
            _dbCon.Close();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Pending>")]
        private static string[] ReadFromDB(string tablename, string column, string value)
        {
            var detailsFromDB = new string[4];
            detailsFromDB[0] = tablename;
            _dbCon.Open();
            try
            {
                using (SQLiteCommand command = new SQLiteCommand(_dbCon))
                {
                    command.CommandText = $"SELECT * FROM {tablename} WHERE {column} = @value";
                    command.Parameters.AddWithValue("@value", value);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        try
                        {
                            while (reader.Read())
                            {
                                detailsFromDB[1] = reader.GetString(0);
                                detailsFromDB[2] = reader.GetString(1);
                                detailsFromDB[3] = reader.GetValue(2).ToString();
                            }
                        }
                        catch (Exception exx)
                        {
                            StatusMessage statusMessage = new StatusMessage();
                            statusMessage.Log("Checking reader: " + exx);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage.PostToActivityBox("File deails in DB: " + ex, MessageType.ERROR);
                StatusMessage logger = new StatusMessage();
                logger.Log($"DBController.ReadFromDB with {tablename} as the table and {column} as the column. Exception: \n" + ex);
            }
            finally
            {
                _dbCon.Close();
            }
            _dbCon.Close();
            return detailsFromDB;
        }

        private static List<EFile> ReadAllStoredFiles()
        {
            var storedFilesList = new List<EFile>();
            _dbCon.Open();

            try
            {
                using (var command = new SQLiteCommand(_dbCon))
                {
                    command.CommandText = "SELECT * FROM files WHERE filestate='true' LIMIT 10";
                    using (var reader = command.ExecuteReader())
                    {
                        try
                        {
                            while (reader.Read())
                            {
                                var filename = reader.GetString(1);
                                var fileInfo = new FileInfo(filename);
                                storedFilesList.Add(new EFile()
                                {
                                    FileName = fileInfo.Name,
                                    FileLocation = filename
                                });
                            }
                        }
                        catch (Exception readerEx)
                        {
                            StatusMessage.PostToActivityBox("Failed to read file list from DB.", MessageType.ERROR);
                            var loggerReader = new StatusMessage();
                            loggerReader.Log($"DBController.ReadAllStoredFiles: {readerEx}");
                        }
                    }
                }
            }
            catch (Exception readFromDB)
            {
                StatusMessage.PostToActivityBox("Failed to fetch file list from DB.", MessageType.ERROR);
                var logger = new StatusMessage();
                logger.Log($"DBController.ReadAllStoredFiles: {readFromDB}");
            }
            finally
            {
                _dbCon.Close();
            }

            _dbCon.Close();

            return storedFilesList;
        }


        public static List<EFile> LoadSavedFiles()
        {
            List<EFile> savedFiles = new List<EFile>();
            savedFiles = ReadAllStoredFiles();

            return savedFiles;
        }

        public static EFile ReadFileDetails(string filename)
        {
            var filehash = new FileHash(filename);
            var normalFile = ReadFromDB("files", "filename", filename);
            var filedetails = new EFile()
            {
                FileHash = normalFile[1],
                FileLocation = normalFile[2],
                IsStored = Convert.ToBoolean(normalFile[3])
            };

            if (!string.IsNullOrEmpty(filedetails.FileLocation))
            {
                var info = new FileInfo(filedetails.FileLocation);
                filedetails.FileName = info.Name;
            }

            return filedetails;
        }

        public static EFile ReadEncryptedFileDetails(string encryptedHash)
        {
            var encryptedFile = ReadFromDB("storedfiles", "encryptedhash", encryptedHash);
            var fileDetails = new EFile()
            {
                FileHash = encryptedFile[1],
                EncryptedHash = encryptedFile[2],
                StoredDateTime = DateTime.Parse(encryptedFile[3])
            };

            return fileDetails;
        }

        public static bool IsFileInDB(string filename)
        {
            //var filesHash = new FileHash(filename);
            var temp = ReadFromDB("files", "filename", filename);
            return Convert.ToBoolean(temp[3]);
        }
    }
}
