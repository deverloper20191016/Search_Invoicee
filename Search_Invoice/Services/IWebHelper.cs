using System.Web;

namespace Search_Invoice.Services
{
    public interface IWebHelper
    {
        string GetUser();
        string GetDvcs();
        string GetLanguage();
        HttpRequestBase GetRequest();
    }
}