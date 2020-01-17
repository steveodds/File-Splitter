namespace P2P_File_Sharing
{
    public class FileHash
    {
        private string _fileName;
        private string _hash;
        private string _formattedHash;
        public FileHash(string fileName)
        {
            _fileName = fileName;
        }

        public string GenerateFileHash()
        {
            //TODO Generate hash of file

            return _hash;
        }

        public string FormatFileHash()
        {
            if (string.IsNullOrEmpty(_hash))
                GenerateFileHash();

            //TODO Format hash string
            return _formattedHash;
        }
    }
}
