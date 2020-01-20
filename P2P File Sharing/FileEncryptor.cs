using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Windows.Forms;

namespace P2P_File_Sharing
{
    public class FileEncryptor
    {
        private readonly string _fileName;
        private readonly string _encryptedHash;
        private readonly string _password;
        private readonly string _encryptedFilename;


        [DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        private static extern bool ZeroMemory(IntPtr Destination, int length);
        public FileEncryptor(EFile fileData)
        {
            //TODO use internal methods to check if file is encrypted or not
            _fileName = fileData.FileName;
            _password = fileData.FileHash;
            if (fileData.IsStored)
            {
                FileInfo fileInfo = new FileInfo(_fileName);

                _encryptedFilename = _fileName.Substring(0, _fileName.Length - (fileInfo.Extension.Length + 1)) + ".aes";
            }
        }

        private static byte[] GenerateRandomSalt()
        {
            var data = new byte[32];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < 10; i++)
                {
                    rng.GetBytes(data);
                }
            }

            return data;
        }

        public void FileEncrypt()
        {
            var salt = GenerateRandomSalt();
            FileStream fsCrypt = new FileStream(_fileName + ".aes", FileMode.Create);

            var passwordBytes = System.Text.Encoding.UTF8.GetBytes(_password);

            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.PKCS7;

            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);

            AES.Mode = CipherMode.CFB;

            fsCrypt.Write(salt, 0, salt.Length);

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);

            FileStream fsIn = new FileStream(_fileName, FileMode.Open);

            var buffer = new byte[1048576];
            int read;

            try
            {
                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                {
                    Application.DoEvents();
                    cs.Write(buffer, 0, read);
                }

                fsIn.Close();
            }
            catch (Exception ex)
            {
                StatusMessage.PostToActivityBox("In FileEncrypt: " + ex, MessageType.ERROR);
            }
            finally
            {
                cs.Close();
                fsCrypt.Close();
            }
        }


        public void FileDecrypt()
        {
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(_password);
            byte[] salt = new byte[32];

            FileStream fsCrypt = new FileStream(_encryptedFilename, FileMode.Open);
            fsCrypt.Read(salt, 0, salt.Length);

            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Padding = PaddingMode.PKCS7;
            AES.Mode = CipherMode.CFB;

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);

            FileStream fsOut = new FileStream(_fileName, FileMode.Create);

            int read;
            byte[] buffer = new byte[1048576];

            try
            {
                while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    Application.DoEvents();
                    fsOut.Write(buffer, 0, read);
                }
            }
            catch (CryptographicException ex_CryptographicException)
            {
                StatusMessage.PostToActivityBox("CryptographicException error: " + ex_CryptographicException.Message, MessageType.ERROR);
            }
            catch (Exception ex)
            {
                StatusMessage.PostToActivityBox("File Decryption: " + ex.Message, MessageType.ERROR);
            }

            try
            {
                cs.Close();
            }
            catch (Exception ex)
            {
                StatusMessage.PostToActivityBox("Error closing CryptoStream: " + ex.Message, MessageType.ERROR);
            }
            finally
            {
                fsOut.Close();
                fsCrypt.Close();
            }
        }




        /* Checker methods */
        public bool IsAlreadyDecrypted()
        {
            //TODO check state in db

            return false;
        }
        public bool IsAlreadyEncrypted()
        {
            //TODO check state in db

            return false;
        }
    }
}
