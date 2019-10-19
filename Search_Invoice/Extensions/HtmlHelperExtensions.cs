using Newtonsoft.Json;
using Search_Invoice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Search_Invoice.Extensions
{
    public static class HtmlHelperExtensions
    {

        public static MvcHtmlString HtmlConvertToJson(this HtmlHelper htmlHelper,
object model)
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };
            return new MvcHtmlString(JsonConvert.SerializeObject(model, settings));
        }

        public static string MD5Hash(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            //compute hash from the bytes of text
            md5.ComputeHash(ASCIIEncoding.UTF8.GetBytes(text));

            //get hash result after compute it
            byte[] result = md5.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                //change it into 2 hexadecimal digits
                //for each byte
                strBuilder.Append(result[i].ToString("x2"));
            }

            return strBuilder.ToString();
        }

        public static string KendoFilter(KendoFilter filterField)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < filterField.Filters.Count; i++)
            {

                builder.Append(filterField.Filters[i].Field + " " + filterField.Filters[i].SqlOperator);

                if (i != filterField.Filters.Count - 1)
                {
                    builder.Append(" " + filterField.Logic);
                }
            }

            return builder.ToString();
        }
    }
}