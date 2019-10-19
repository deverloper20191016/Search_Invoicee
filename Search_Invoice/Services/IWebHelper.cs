using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Search_Invoice.Services
{
    public partial interface IWebHelper
    {
        string GetUser();
        string GetDvcs();
        string GetLanguage();
        HttpRequestBase GetRequest();
    }
}