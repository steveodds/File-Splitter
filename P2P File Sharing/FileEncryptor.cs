namespace P2P_File_Sharing
{
    public class FileEncryptor
    {
        private readonly string _fileName;
        private readonly string _encryptedHash;
        public FileEncryptor(string filename)
        {
            _fileName = filename;
        }

        public void Encrypt()
        {
            //TODO encrypt file
        }

        public bool IsAlreadyEncrypted()
        {
            //TODO check state in db

            return false;
        }

        public void Decrypt()
        {
            //TODO decrypt file
        }

        public bool IsAlreadyDecrypted()
        {
            //TODO check state in db

            return false;
        }
    }
}
