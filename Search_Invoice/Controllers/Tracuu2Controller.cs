using Newtonsoft.Json.Linq;
using Search_Invoice.Services;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Search_Invoice.Authorization;

namespace Search_Invoice.Controllers
{
    public class Tracuu2Controller : ApiController
    {
        private readonly ITracuuService2 _tracuuService2;
        public Tracuu2Controller(ITracuuService2 tracuuService2)
        {
            _tracuuService2 = tracuuService2;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Tracuu2/GetInfoInvoice")]
        public JObject GetInfoInvoice(JObject model)
        {
            JObject json = _tracuuService2.GetInfoInvoice(model);
            return json;
        }

        [HttpPost]
        [Route("Tracuu2/PrintInvoice")]
        [AllowAnonymous]
        public HttpResponseMessage PrintInvoice(JObject model)
        {
            HttpResponseMessage result;
            try
            {
                string type = model["type"].ToString();
                string sobaomat = model["sobaomat"].ToString();
                bool inchuyendoi = model.ContainsKey("inchuyendoi");
               
                string originalString = ActionContext.Request.RequestUri.OriginalString;
                string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
                var folder = System.Web.HttpContext.Current.Server.MapPath(path);
                byte[] bytes = _tracuuService2.PrintInvoiceFromSbm(sobaomat, folder, type, inchuyendoi);
                result = new HttpResponseMessage(HttpStatusCode.OK) {Content = new ByteArrayContent(bytes)};
                if (type == "PDF")
                {
                    result.Content.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue("inline") {FileName = "InvoiceTemplate.pdf"};
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
                result = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(ex.Message, System.Text.Encoding.UTF8)
                };
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                result.Content.Headers.ContentLength = ex.Message.Length;
            }
            return result;
        }

        [HttpPost]
        [Route("Tracuu2/ExportZipFileXML")]
        [AllowAnonymous]
        public HttpResponseMessage ExportZipFileXml(JObject model)
        {
            HttpResponseMessage result;
            try
            {
                string sobaomat = model["sobaomat"].ToString();
                string originalString = ActionContext.Request.RequestUri.OriginalString;
                string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
                
                var folder = System.Web.HttpContext.Current.Server.MapPath(path);
                string fileName = "";
                string key = "";
                byte[] bytes = _tracuuService2.ExportZipFileXml(sobaomat, folder, ref fileName, ref key);
                result = new HttpResponseMessage(HttpStatusCode.OK) {Content = new ByteArrayContent(bytes)};
                result.Content.Headers.ContentDisposition =
                    new ContentDispositionHeaderValue("attach") {FileName = fileName + ".zip"};
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
                result.Content.Headers.ContentLength = bytes.Length;
            }
            catch (Exception ex)
            {
                result = new HttpResponseMessage(HttpStatusCode.BadRequest) {Content = new StringContent(ex.Message)};
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                result.Content.Headers.ContentLength = ex.Message.Length;
            }
            return result;
        }

        [HttpPost]
        [Route("Tracuu2/ExportZipFile")]
        [Authorize]
        [BaseAuthentication]
        public HttpResponseMessage ExportZipFile(JObject model)
        {
            HttpResponseMessage result;
            try
            {
                string sobaomat = model["sobaomat"].ToString();
                string originalString = ActionContext.Request.RequestUri.OriginalString;
                string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
                var folder = System.Web.HttpContext.Current.Server.MapPath(path);
                string fileName = "";
                string key = "";
                byte[] bytes = _tracuuService2.ExportZipFileXml(sobaomat, folder, ref fileName, ref key);
                result = new HttpResponseMessage(HttpStatusCode.OK) {Content = new ByteArrayContent(bytes)};
                result.Content.Headers.ContentDisposition =
                    new ContentDispositionHeaderValue("attach") {FileName = fileName + ".zip"};
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                result.Content.Headers.ContentLength = bytes.Length;
            }
            catch (Exception ex)
            {
                result = new HttpResponseMessage(HttpStatusCode.BadRequest) {Content = new StringContent(ex.Message)};
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                result.Content.Headers.ContentLength = ex.Message.Length;
            }
            return result;
        }

        [HttpPost]
        [Route("tracuu2/exportpdffile")]
        [Authorize]
        [BaseAuthentication]
        public HttpResponseMessage ExportPdfFile(JObject model)
        {
            HttpResponseMessage result;
            try
            {
                string sobaomat = model["sobaomat"].ToString();
                string originalString = ActionContext.Request.RequestUri.OriginalString;
                string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
                var folder = System.Web.HttpContext.Current.Server.MapPath(path);
                string fileName = "";
                byte[] bytes = _tracuuService2.PrintInvoiceFromSbm(sobaomat, folder, "pdf");
                result = new HttpResponseMessage(HttpStatusCode.OK) {Content = new ByteArrayContent(bytes)};
                result.Content.Headers.ContentDisposition =
                    new ContentDispositionHeaderValue("attach") {FileName = fileName + ".pdf"};
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                result.Content.Headers.ContentLength = bytes.Length;
            }
            catch (Exception ex)
            {
                result = new HttpResponseMessage(HttpStatusCode.BadRequest) {Content = new StringContent(ex.Message)};
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                result.Content.Headers.ContentLength = ex.Message.Length;
            }
            return result;
        }

        

       

        [HttpPost]
        [Route("Tracuu2/PrintInvoicePdf")]
        [AllowAnonymous]
        public JObject PrintInvoicePdf(JObject model)
        {
            JObject result = new JObject();
            try
            {
                string type = model["type"].ToString();
                string sobaomat = model["sobaomat"].ToString();            
                string originalString = ActionContext.Request.RequestUri.OriginalString;
                string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
                var folder = System.Web.HttpContext.Current.Server.MapPath(path);
                string xml;
                string fileName;
                byte[] bytes = _tracuuService2.PrintInvoiceFromSbm(sobaomat, folder, type, false, out xml, out fileName);
                string a = Convert.ToBase64String(bytes);
                result.Add("ok", a);
                result.Add("ecd", xml);
                result.Add("fileName", fileName);
            }
            catch (Exception ex)
            {
                result.Add("error", ex.Message);
            }

            return result;
        }

        [HttpPost]
        [Route("tracuu2/getinvoicexml")]
        [AllowAnonymous]
        public HttpResponseMessage GetInvoiceXml(JObject model)
        {
            HttpResponseMessage result;
            try
            {
                string sobaomat = model["sobaomat"].ToString();
                byte[] bytes = _tracuuService2.GetInvoiceXml(sobaomat);
                result = new HttpResponseMessage(HttpStatusCode.OK) {Content = new ByteArrayContent(bytes)};
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attach")
                {
                    FileName = "invoice.xml"
                };
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
                result.Content.Headers.ContentLength = bytes.Length;
            }
            catch (Exception ex)
            {
                result = new HttpResponseMessage(HttpStatusCode.BadRequest) {Content = new StringContent(ex.Message)};
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                result.Content.Headers.ContentLength = ex.Message.Length;
            }
            return result;
        }
      
    }
}
