using System;
using System.Text;
using System.Web;
using System.Web.Security;
using Search_Invoice.DAL;

namespace Search_Invoice.Services
{
    public class WebHelper : IWebHelper
    {
        private readonly HttpContextBase _context;

        public WebHelper(HttpContextBase context)
        {
            _context = context;
        }

        public HttpRequestBase GetRequest()
        {
            return _context.Request;
        }

        public string GetUser()
        {
            //string authorization = _context.Request.Headers["Authorization"];
            //if (string.IsNullOrEmpty(authorization))
            //{
            //    return null;
            //}
            //string[] array = authorization.Replace("Bear", "").Trim().Split(';');

            //if (array.Length == 0)
            //{
            //    return null;
            //}
            //string token = array[0];
            //string key = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            //string[] parts = key.Split(new char[] { ':' });
            //return parts[1];

            var authCookie = _context.Request.Cookies[CommonConstants.UserCookie];
            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (authTicket != null && !authTicket.Expired)
                {
                    return authTicket.Name;
                }
            }

            return null;
        }


        public string GetDvcs()
        {
            string authorization = _context.Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorization))
            {
                return null;
            }
            string[] array = authorization.Split(';');
            if (array.Length < 2)
            {
                return null;
            }
            string maDvcs = array[1];
            return maDvcs;
        }

        public string GetLanguage()
        {
            string authorization = _context.Request.Headers["Authorization"];
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