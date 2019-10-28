using Newtonsoft.Json.Linq;
using Search_Invoice.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace Search_Invoice.Controllers
{
    public class Tracuu2Controller : ApiController
    {
        private ITracuuService2 _tracuuService2;

        public Tracuu2Controller(ITracuuService2 tracuuService2)
        {
            this._tracuuService2 = tracuuService2;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Tracuu2/nam1")]
        public JArray nam1()
        {
            JArray jarr = new JArray() { "avvvvvvv", "aaaaaaaaaaaa" };
            return jarr;
        }

        //[HttpPost]
        //[AllowAnonymous]
        //[Route("Tracuu2/GetInvoiceFromdateTodate")]
        //public JObject GetInvoiceFromdateTodate(JObject model)
        //{
        //    Session
        //    JObject json = _tracuuService2.GetInfoInvoice(model);
        //    return json;
        //}

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

            HttpResponseMessage result = null;
            try
            {
                string type = model["type"].ToString();
                string sobaomat = model["sobaomat"].ToString();
                string masothue = model["masothue"].ToString();
                //if (_tracuuService2 == null)
                //{
                //    throw new Exception("Không tồn tại mst:");
                //}
                //string type = "PDF";
                string originalString = this.ActionContext.Request.RequestUri.OriginalString;
                string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
                //string path = "~/Content/report/";
                var folder = System.Web.HttpContext.Current.Server.MapPath(path);

                byte[] bytes = _tracuuService2.PrintInvoiceFromSBM(sobaomat, masothue, folder, type);

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
                result = new HttpResponseMessage(HttpStatusCode.BadRequest);
                result.Content = new StringContent(ex.Message, System.Text.Encoding.UTF8);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                result.Content.Headers.ContentLength = ex.Message.Length;
            }

            return result;
        }

        [HttpPost]
        [Route("Tracuu2/ExportZipFileXML")]
        public HttpResponseMessage ExportZipFileXML(JObject model)
        {

            HttpResponseMessage result = null;

            try
            {
                string masothue = model["masothue"].ToString();
                string sobaomat = model["sobaomat"].ToString();
                string originalString = this.ActionContext.Request.RequestUri.OriginalString;
                string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";

                var folder = System.Web.HttpContext.Current.Server.MapPath(path);

                string fileName = "";
                string key = "";
                byte[] bytes = _tracuuService2.ExportZipFileXML(sobaomat, masothue, folder, ref fileName, ref key);

                result = new HttpResponseMessage(HttpStatusCode.OK);

                result.Content = new ByteArrayContent(bytes);
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attach");
                result.Content.Headers.ContentDisposition.FileName = fileName + ".zip";
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
                result.Content.Headers.ContentLength = bytes.Length;
            }
            catch (Exception ex)
            {
                result = new HttpResponseMessage(HttpStatusCode.BadRequest);
                result.Content = new StringContent(ex.Message);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                result.Content.Headers.ContentLength = ex.Message.Length;
            }

            return result;
        }


        [HttpPost]
        [Route("tracuu2/gethtml")]
        [AllowAnonymous]
        public JObject GetHtml(JObject model)
        {
            string originalString = this.ActionContext.Request.RequestUri.OriginalString;
            string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";

            var folder = System.Web.HttpContext.Current.Server.MapPath(path);
            model.Add("folder", folder);
            return _tracuuService2.GetHtml(model);
        }

        [HttpGet]
        [Route("tracuu2/searchTax")]
        [AllowAnonymous]
        public JObject searchTax(String model)
        {
            
            return _tracuuService2.Search_Tax(model);
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
                string masothue = model["masothue"].ToString();
                //if (_tracuuService2 == null)
                //{
                //    throw new Exception("Không tồn tại mst:");
                //}
                //string type = "PDF";
                string originalString = this.ActionContext.Request.RequestUri.OriginalString;
                string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
                //string path = "~/Content/report/";
                var folder = System.Web.HttpContext.Current.Server.MapPath(path);

                string xml = "";

                byte[] bytes = _tracuuService2.PrintInvoiceFromSBM(sobaomat, masothue, folder, type, out xml);

                string a = Convert.ToBase64String(bytes);
                result.Add("ok", a);
                result.Add("ecd", xml);
            }
            catch (Exception ex)
            {
                result.Add("error", ex.Message);
            }

            return result;
        }
    }
}
