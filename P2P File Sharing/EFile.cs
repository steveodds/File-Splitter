using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2P_File_Sharing
{
    public class EFile
    {
        public string FileName { get; set; }
        public string FileLocation { get; set; }
        public string FileHash { get; set; }
        public bool IsStored { get; set; }
        public string EncryptedHash { get; set; }
        public DateTime StoredDateTime { get; set; }

        public EFile()
        {

        }
        public EFile(FileInfo fileInfo)
        {
            FileName = fileInfo.Name;
            FileLocation = fileInfo.FullName;
            IsStored = IsFileInDB(FileLocation);
            FileHash = GenerateHash(FileLocation);
        }

        private string GenerateHash(string fileName)
        {
            var getHash = new FileHash(fileName);
            var hash = getHash.GenerateFileHash();
            return hash;
        }

        private bool IsFileInDB(string fileName)
        {
            return DBController.IsFileInDB(fileName);
        }
        
        public string[] ToStringArray()
        {
            var fileDetailsAsArray = new string[3];
            fileDetailsAsArray[0] = FileHash;
            fileDetailsAsArray[1] = FileLocation;
            fileDetailsAsArray[2] = IsStored.ToString();

            return fileDetailsAsArray;
        }

        public string[] ToStringArrayEncrypted()
        {
            var fileDetailsAsArray = new string[3];
            fileDetailsAsArray[0] = FileHash;
            fileDetailsAsArray[1] = EncryptedHash;
            fileDetailsAsArray[2] = StoredDateTime.ToString();

            return fileDetailsAsArray;
        }
    }
}
