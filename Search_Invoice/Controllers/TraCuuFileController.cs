using Newtonsoft.Json.Linq;
using Search_Invoice.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Search_Invoice.Controllers
{
    public class TraCuuFileController : ApiController
    {
        [HttpPost]
        [Route("TracuuFile/UploadInv")]
        [AllowAnonymous]
        public HttpResponseMessage UploadInv()
        {
            Stream streams = new MemoryStream();
            string type = "PDF";
            string fileName = null;
            try
            {
                var httpRequest = HttpContext.Current.Request;
                if (httpRequest.Files.Count < 1)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Chưa chọn file hóa đơn");
                }
                foreach (string file in httpRequest.Files)
                {
                    var postedFile = httpRequest.Files[file];
                    streams = postedFile.InputStream;
                    fileName = postedFile.FileName;
                }
                var json = new JObject();

                if (!fileName.EndsWith("zip"))
                {
                    json.Add("error", "File tải lên không đúng *.zip");
                    json.Add("status", "server");

                    return Request.CreateResponse(HttpStatusCode.BadRequest, json); ;
                }
                string xml = "";
                string repx = "";
                string key = "";
                ReportUtil.ExtracInvoice(streams, ref xml, ref repx, ref key);
                string xmlDecryp = EncodeXml.Decrypt(xml, key);
                string originalString = this.ActionContext.Request.RequestUri.OriginalString;
                string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
                string folder = HttpContext.Current.Server.MapPath(path);
                byte[] buffer = ReportUtil.InvoiceReport(xmlDecryp, repx, folder, "PDF");
                var result = new HttpResponseMessage(HttpStatusCode.OK) {Content = new ByteArrayContent(buffer)};
                if (type == "PDF")
                {
                    result.Content.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue("inline") {FileName = "Invoice.pdf"};
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                }
                result.Content.Headers.ContentLength = buffer.Length;
                return result;
            }
            catch (Exception ex)
            {
                var json = new JObject { { "error", ex.Message }, { "status", "server" } };
                return Request.CreateResponse(HttpStatusCode.BadRequest, json); ;
            }
        }


        [HttpPost]
        [Route("TracuuFile/VeryfyXml")]
        [AllowAnonymous]
        public HttpResponseMessage VeryfyXml()
        {
            var json = new JObject();
            Stream streams = new MemoryStream();
            string fileName = null;
            try
            {
                var httpRequest = HttpContext.Current.Request;
                if (httpRequest.Files.Count < 1)
                {
                    json.Add("error", "Chưa chọn file hóa đơn");
                    return Request.CreateResponse(HttpStatusCode.BadRequest, json);
                }
                foreach (string file in httpRequest.Files)
                {
                    var postedFile = httpRequest.Files[file];
                    streams = postedFile.InputStream;
                    fileName = postedFile.FileName;
                }

                if (!fileName.EndsWith("zip"))
                {
                    json.Add("error", "File tải lên không đúng *.zip");
                    json.Add("status", "server");
                    return Request.CreateResponse(HttpStatusCode.BadRequest, json);
                }
                string xml = "";
                string repx = "";
                string key = "";
                ReportUtil.ExtracInvoice(streams, ref xml, ref repx, ref key);
                string xmlDecryp = EncodeXml.Decrypt(xml, key);
                var result = ReportUtil.VeryfyXml(xmlDecryp);
                return Request.CreateResponse(HttpStatusCode.BadRequest, result);
            }
            catch (Exception ex)
            {
                json.Add("error", ex.Message);
                return Request.CreateResponse(HttpStatusCode.BadRequest, json);
            }
        }
    }
}
