using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using Search_Invoice.Services;
using System.Threading.Tasks;
using Search_Invoice.Util;
using System.IO;

namespace Search_Invoice.Controllers
{
    public class SearchInvoiceController : ApiController
    {
        private ITracuuService _tracuuService;

        public SearchInvoiceController(ITracuuService tracuuService)
        {
            this._tracuuService = tracuuService;
        }

        [HttpGet]
        [Route("Tracuu/TaxPath")]
        [AllowAnonymous]
        public string TaxPath(string model)
        {
            JObject json = new JObject();
            json.Add("error", "");

            try
            {
                json.Add("ok", true);
            }
            catch (Exception ex)
            {
                json["error"] = ex.Message;
            }

            return json.ToString();
        }
        [HttpGet]
        [Route("Tracuu/TaxPath1")]
        [AllowAnonymous]
        public JObject TaxPath1(JObject model)
        {
            JObject json = new JObject();
            json.Add("error", "");

            try
            {
                json.Add("ok", true);
            }
            catch (Exception ex)
            {
                json["error"] = ex.Message;
            }

            return json;
        }
        [HttpGet]
        [Route("Tracuu/M-Invoice")]
        [AllowAnonymous]
        public HttpResponseMessage PrintInvoice(string mst, string id)
        {

            HttpResponseMessage result = null;
            try
            {
                if(_tracuuService == null)
                {
                    throw new Exception("Không tồn tại mst:");
                }
                string type = "PDF";
                string originalString = this.ActionContext.Request.RequestUri.OriginalString;
                string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
                //string path = "~/Content/report/";
                var folder = System.Web.HttpContext.Current.Server.MapPath(path);

                byte[] bytes = _tracuuService.PrintInvoiceFromSBM(id, folder, type);

                result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new ByteArrayContent(bytes);

                if (type == "PDF")
                {
                    result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");
                    result.Content.Headers.ContentDisposition.FileName = "InvoiceTemplate.pdf";
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                }
                else if (type == "Html")
                {
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                }
                result.Content.Headers.ContentLength = bytes.Length;
            }
            catch (Exception ex)
            {
                result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StringContent(ex.Message, System.Text.Encoding.UTF8);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                result.Content.Headers.ContentLength = ex.Message.Length;
            }

            return result;
        }
    }
}
