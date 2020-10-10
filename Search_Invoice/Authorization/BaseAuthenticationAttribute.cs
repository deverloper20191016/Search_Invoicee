using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using Newtonsoft.Json.Linq;

namespace Search_Invoice.Authorization
{
    public class BaseAuthenticationAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            bool skipAuthorization = actionContext.ActionDescriptor.ActionName.Contains("ExportZipFileXML");
            if (!actionContext.RequestContext.Principal.Identity.IsAuthenticated && !skipAuthorization)
            {
                actionContext.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized };
                var json = new JObject
                {
                    {"status_code", 401 }
                };
                actionContext.Response.Content = new StringContent(json.ToString(),Encoding.UTF8, "application/json");
            }
        }
    }
}