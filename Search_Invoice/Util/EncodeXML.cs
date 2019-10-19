using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Search_Invoice.Util
{
    public class EncodeXML
    {
        public static string Encrypt(string data, string key)
        {
            //string password1 = key + "nampv269";
            string password1 = key;
            // Encode message and password
            byte[] messageBytes = Encoding.UTF8.GetBytes(data);
            //byte[] passwordBytes = Encoding.UTF8.GetBytes(password1);
            MD5CryptoServiceProvider hashMD5Provider = new MD5CryptoServiceProvider();

            byte[] passwordBytes;
            //byte[] byteBuff;
            //string key1 = key + "nampv269";
            passwordBytes = hashMD5Provider.ComputeHash(Encoding.UTF8.GetBytes(password1));
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
                //string password1 = key + "nampv269";
                string password1 = key;
                // Encode message and password

                byte[] encryptedMessageBytes = Convert.FromBase64String(data);
                //byte[] passwordBytes = Encoding.UTF8.GetBytes(password1);

                byte[] passwordBytes;
                //byte[] byteBuff;
                //string key1 = key + "nampv269";
                MD5CryptoServiceProvider hashMD5Provider = new MD5CryptoServiceProvider();
                passwordBytes = hashMD5Provider.ComputeHash(Encoding.UTF8.GetBytes(password1));

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