using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Search_Invoice.Util
{
    public class EncodeXml
    {
        public static string Encrypt(string data, string key)
        {
            //string password1 = key + "nampv269";
            string password1 = key;
            // Encode message and password
            byte[] messageBytes = Encoding.UTF8.GetBytes(data);
            //byte[] passwordBytes = Encoding.UTF8.GetBytes(password1);
            MD5CryptoServiceProvider hashMd5Provider = new MD5CryptoServiceProvider();
            //byte[] byteBuff;
            //string key1 = key + "nampv269";
            var passwordBytes = hashMd5Provider.ComputeHash(Encoding.UTF8.GetBytes(password1));
            // Set encryption settings -- Use password for both key and init. vector
            TripleDESCryptoServiceProvider provider = new TripleDESCryptoServiceProvider();
            ICryptoTransform transform = provider.CreateEncryptor(passwordBytes, passwordBytes);
            CryptoStreamMode mode = CryptoStreamMode.Write;
            // Set up streams and encrypt
            MemoryStream memStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memStream, transform, mode);
            cryptoStream.Write(messageBytes, 0, messageBytes.Length);
            cryptoStream.FlushFinalBlock();

            // Read the encrypted message from the memory stream
            byte[] encryptedMessageBytes = new byte[memStream.Length];
            memStream.Position = 0;
            memStream.Read(encryptedMessageBytes, 0, encryptedMessageBytes.Length);

            // Encode the encrypted message as base64 string
            string encryptedMessage = Convert.ToBase64String(encryptedMessageBytes);

            return encryptedMessage;
        }

        public static string Decrypt(string data, string key)
        {
            if (string.IsNullOrEmpty(key)) return data;
            try
            {
                // Convert encrypted message and password to bytes
                string password1 = key;
                // Encode message and password
                byte[] encryptedMessageBytes = Convert.FromBase64String(data);
                //byte[] byteBuff;
                MD5CryptoServiceProvider hashMd5Provider = new MD5CryptoServiceProvider();
                var passwordBytes = hashMd5Provider.ComputeHash(Encoding.UTF8.GetBytes(password1));
                // Set encryption settings -- Use password for both key and init. vector
                TripleDESCryptoServiceProvider provider = new TripleDESCryptoServiceProvider();
                ICryptoTransform transform = provider.CreateDecryptor(passwordBytes, passwordBytes);
                CryptoStreamMode mode = CryptoStreamMode.Write;
                // Set up streams and decrypt
                MemoryStream memStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memStream, transform, mode);
                cryptoStream.Write(encryptedMessageBytes, 0, encryptedMessageBytes.Length);
                cryptoStream.FlushFinalBlock();
                // Read decrypted message from memory stream
                byte[] decryptedMessageBytes = new byte[memStream.Length];
                memStream.Position = 0;
                memStream.Read(decryptedMessageBytes, 0, decryptedMessageBytes.Length);
                // Encode deencrypted binary data to base64 string
                string message = Encoding.UTF8.GetString(decryptedMessageBytes);
                //string message = Convert.ToBase64String(decryptedMessageBytes);
                return message;
            }
            catch (Exception)
            {
                throw new Exception("Hóa đơn không đúng hoặc đã bị chỉnh sửa");
            }
        }
    }
}