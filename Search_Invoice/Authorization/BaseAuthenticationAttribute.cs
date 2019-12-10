using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Newtonsoft.Json.Linq;

namespace Search_Invoice.Authorization
{
    public class BaseAuthenticationAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {


            base.OnAuthorization(actionContext);
        }

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

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {

            return base.IsAuthorized(actionContext);
        }
    }
}