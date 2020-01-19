namespace P2P_File_Sharing
{
    public class StoreFile
    {
        private EFile _file;
        private string _customLocation;

        public StoreFile(EFile file)
        {
            _file = file;
            Store();
        }

        private void Store()
        {
            if (!_file.IsStored)
            {
                DBController.WriteToDB("storedfiles", _file);
            }
        }
    }
}
