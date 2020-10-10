using System;
using System.Text;
using System.Security.Cryptography;

namespace Search_Invoice.Util
{
    public class PassCommand
    {
        private readonly string _passFromDatabase;

        public PassCommand(string password)
        {
            _passFromDatabase = password;
        }

        public bool CheckPassword(string password)
        {
            byte[] sourceArray = Convert.FromBase64String(Password);
            int sourceIndex = 0x40;
            int length = sourceArray.Length - sourceIndex;
            byte[] destinationArray = new byte[length];
            Array.Copy(sourceArray, sourceIndex, destinationArray, 0, length);
            string str = CreateHashedPassword(password, destinationArray);
            return (_passFromDatabase == str);
        }

        public string CreateHashedPassword(string password, byte[] existingSalt)
        {
            byte[] data;
            if (existingSalt == null)
            {
                Random random = new Random();
                data = new byte[random.Next(0x10, 0x40)];
                new RNGCryptoServiceProvider().GetNonZeroBytes(data);
            }
            else
            {
                data = existingSalt;
            }
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] array = new byte[bytes.Length + data.Length];
            bytes.CopyTo(array, 0);
            data.CopyTo(array, bytes.Length);
            byte[] buffer4 = new SHA512Managed().ComputeHash(array);
            byte[] buffer5 = new byte[buffer4.Length + data.Length];
            buffer4.CopyTo(buffer5, 0);
            data.CopyTo(buffer5, buffer4.Length);
            return Convert.ToBase64String(buffer5);
        }

        public string Password
        {
            get
            {
                return _passFromDatabase;
            }
        }
    }
}