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
        [Route("TracuuFile/UploadInvAPI")]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> UploadInvAPI()
        {
            HttpResponseMessage result = null;
            string type = "PDF";
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            byte[] bytes = null;
            string dmmauhoadon_id = null;
            string fileName = null;

            try
            {
                var filesReadToProvider = await Request.Content.ReadAsMultipartAsync();

                foreach (var stream in filesReadToProvider.Contents)
                {
                    if (stream.IsFormData())
                    {
                        var s = await stream.ReadAsFormDataAsync();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(stream.Headers.ContentDisposition.FileName))
                        {
                            bytes = await stream.ReadAsByteArrayAsync();
                            fileName = stream.Headers.ContentDisposition.FileName.Replace("\"", "");
                        }
                        else
                        {
                            dmmauhoadon_id = await stream.ReadAsStringAsync();
                        }
                    }
                }

                var json = new JObject();

                if (!fileName.EndsWith("zip"))
                {
                    json.Add("error", "File tải lên không đúng");
                    json.Add("status", "server");

                    return Request.CreateResponse(HttpStatusCode.OK, json); ;
                }

                // string result = await _invoiceService.UploadInvTemplate(this.Ma_dvcs, this.UserName, dmmauhoadon_id, bytes);
                string xml = "";
                string repx = "";
                string key = "";
                Stream streams = new MemoryStream(bytes);

                ReportUtil.ExtracInvoice(streams, ref xml, ref repx, ref key);
                string xmlDecryp = EncodeXml.Decrypt(xml, key);

                // byte[] buffer = null;
                string originalString = this.ActionContext.Request.RequestUri.OriginalString;
                string folder = originalString.StartsWith("/api") ? "/api/Content/report/" : "/Content/report/";
                //string folder = base.Server.MapPath("~/Content/report/");
                byte[] buffer = ReportUtil.InvoiceReport(xmlDecryp, repx, folder, "PDF");

                result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new ByteArrayContent(bytes);

                if (type == "PDF")
                {
                    result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");
                    result.Content.Headers.ContentDisposition.FileName = "Invoice.pdf";
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                }

                result.Content.Headers.ContentLength = bytes.Length;
                return result;
            }
            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }
        [HttpPost]
        [Route("TracuuFile/UploadInv")]
        [AllowAnonymous]
        public HttpResponseMessage UploadInv()
        {
            HttpResponseMessage result = null;
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
                    //var filePath = HttpContext.Current.Server.MapPath("~/" + postedFile.FileName);
                    //postedFile.SaveAs(filePath);
                    // NOTE: To store in memory use postedFile.InputStream
                }

                // return Request.CreateResponse(HttpStatusCode.Created);

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

                string folder = System.Web.HttpContext.Current.Server.MapPath(path);

                byte[] buffer = ReportUtil.InvoiceReport(xmlDecryp, repx, folder, "PDF");

                result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new ByteArrayContent(buffer);

                if (type == "PDF")
                {
                    result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");
                    result.Content.Headers.ContentDisposition.FileName = "Invoice.pdf";
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                }

                result.Content.Headers.ContentLength = buffer.Length;
                return result;
            }
            catch (Exception ex)
            {
                //result = new HttpResponseMessage(HttpStatusCode.BadRequest)
                //{
                //    Content = new StringContent(new JObject
                //    {
                //        {"error", ex.Message}
                //    }.ToString())
                //};
                //result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                //result.Content.Headers.ContentLength = ex.Message.Length;
                var json = new JObject { { "error", ex.Message }, { "status", "server" } };

                return Request.CreateResponse(HttpStatusCode.BadRequest, json); ;
            }
        }


        [HttpPost]
        [Route("TracuuFile/VeryfyXml")]
        [AllowAnonymous]
        public HttpResponseMessage VeryfyXml()
        {
            HttpResponseMessage kq = null;
            var json = new JObject();
            Stream streams = new MemoryStream();
            string type = "PDF";

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
