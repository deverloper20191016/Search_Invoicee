using System.Security.Cryptography;
using System.Text;

namespace Search_Invoice.Util
{
    public class CommonUtil
    {
        public static string Md5Hash(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            md5.ComputeHash(Encoding.UTF8.GetBytes(text));

            byte[] hash = md5.Hash;
            StringBuilder builder = new StringBuilder();
            foreach (var t in hash)
            {
                builder.Append(t.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}