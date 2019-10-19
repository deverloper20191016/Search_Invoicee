using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Search_Invoice.Services
{
    public class WebHelper : IWebHelper
    {
        private readonly HttpContextBase _context;

        public WebHelper(HttpContextBase context)
        {
            this._context = context;
        }

        public HttpRequestBase GetRequest()
        {
            return this._context.Request;
        }

        public string GetUser()
        {
            string authorization = this._context.Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authorization))
            {
                return null;
            }

            string[] array = authorization.Replace("Bear", "").Trim().Split(';');

            if (array.Length == 0)
            {
                return null;
            }

            string token = array[0];
            string key = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            string[] parts = key.Split(new char[] { ':' });

            return parts[1];
        }


        public string GetDvcs()
        {
            string authorization = this._context.Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authorization))
            {
                return null;
            }

            string[] array = authorization.Split(';');

            if (array.Length < 2)
            {
                return null;
            }

            string ma_dvcs = array[1];

            return ma_dvcs;
        }

        public string GetLanguage()
        {
            string authorization = this._context.Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authorization))
            {
                return null;
            }

            string[] array = authorization.Split(';');

            if (array.Length < 3)
            {
                return null;
            }

            string language = array[2];

            return language;
        }
    }
}