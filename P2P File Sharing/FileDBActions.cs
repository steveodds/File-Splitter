using System;
using System.IO;

namespace P2P_File_Sharing
{
    public class FileDBActions : DBController
    {
        public static bool IsFileInDB(string file)
        {
            _dbCon.Open();
            //TODO check if file exists in DB
            return false;
        }

        public static EFile FileDetailsInDB(string file)
        {
            if (!IsFileInDB(file))
                throw new ArgumentException("The file does not exist in the database.");
            var temp = new FileInfo("nodata.txt");
            //TODO Check file date in db
            return new EFile(temp);
        }
    }
}
