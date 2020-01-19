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

        public EFile()
        {

        }
        public EFile(FileInfo fileInfo)
        {
            FileName = fileInfo.Name;
            FileLocation = fileInfo.FullName;
            IsStored = IsFileInDB(FileName);
            FileHash = GenerateHash(FileName);
        }

        private string GenerateHash(string fileName)
        {
            var getHash = new FileHash(fileName);
            var hash = getHash.GenerateFileHash();
            return hash;
        }

        private bool IsFileInDB(string fileName)
        {
            return FileDBActions.IsFileInDB(fileName);
        }
        
    }
}
