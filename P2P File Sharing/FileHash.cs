using System;
using System.IO;
using System.Security.Cryptography;

namespace P2P_File_Sharing
{
    public class FileHash
    {
        private string _fileName;
        private string _formattedHash;
        public FileHash(string fileName)
        {
            _fileName = fileName;
        }

        public string GenerateFileHash()
        {
            if (_fileName == null)
                throw new ArgumentNullException("The file name cannot be null");

            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(_fileName))
                {
                    _formattedHash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                }
            }
            return _formattedHash;
        }
    }
}
