using Newtonsoft.Json.Linq;
using Search_Invoice.Authorization;
using Search_Invoice.Services;
using Search_Invoice.Util;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Http;

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
                string masothue = model["masothue"].ToString();
                string id = model["auth"].ToString();
                bool inchuyendoi = model.ContainsKey("inchuyendoi");
                string originalString = ActionContext.Request.RequestUri.OriginalString;
                string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
                string folder = HttpContext.Current.Server.MapPath(path);
                byte[] bytes = _tracuuService2.PrintInvoiceFromSbm(id, sobaomat, masothue, folder, type, inchuyendoi);
                result = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
                if (type == "PDF")
                {
                    result.Content.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue("inline") { FileName = "InvoiceTemplate.pdf" };
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
                    Content = new StringContent(ex.Message, Encoding.UTF8)
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
                string masothue = model["masothue"].ToString();
                string sobaomat = model["sobaomat"].ToString();
                string id = model["auth"].ToString();
                string originalString = ActionContext.Request.RequestUri.OriginalString;
                string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
                string folder = HttpContext.Current.Server.MapPath(path);
                string fileName = "";
                string key = "";
                byte[] bytes = _tracuuService2.ExportZipFileXml(id, sobaomat, masothue, folder, ref fileName, ref key);
                result = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
                result.Content.Headers.ContentDisposition =
                    new ContentDispositionHeaderValue("attach") { FileName = fileName + ".zip" };
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
                result.Content.Headers.ContentLength = bytes.Length;
            }
            catch (Exception ex)
            {
                result = new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(ex.Message) };
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
                string masothue = model["masothue"].ToString();
                string sobaomat = model["sobaomat"].ToString();
                string id = model["auth"].ToString();
                string originalString = ActionContext.Request.RequestUri.OriginalString;
                string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
                string folder = HttpContext.Current.Server.MapPath(path);
                string fileName = "";
                string key = "";
                byte[] bytes = _tracuuService2.ExportZipFileXml(id, sobaomat, masothue, folder, ref fileName, ref key);
                result = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
                result.Content.Headers.ContentDisposition =
                    new ContentDispositionHeaderValue("attach") { FileName = fileName + ".zip" };
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                result.Content.Headers.ContentLength = bytes.Length;
            }
            catch (Exception ex)
            {
                result = new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(ex.Message) };
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
                string masothue = model["masothue"].ToString();
                string sobaomat = model["sobaomat"].ToString();
                string id = model["auth"].ToString();
                string originalString = ActionContext.Request.RequestUri.OriginalString;
                string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
                string folder = HttpContext.Current.Server.MapPath(path);
                string fileName = "";
                byte[] bytes = _tracuuService2.PrintInvoiceFromSbm(id, sobaomat, masothue, folder, "pdf");
                result = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
                result.Content.Headers.ContentDisposition =
                    new ContentDispositionHeaderValue("attach") { FileName = fileName + ".pdf" };
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                result.Content.Headers.ContentLength = bytes.Length;
            }
            catch (Exception ex)
            {
                result = new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(ex.Message) };
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
            string originalString = ActionContext.Request.RequestUri.OriginalString;
            string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
            string folder = HttpContext.Current.Server.MapPath(path);
            model.Add("folder", folder);
            return _tracuuService2.GetHtml(model);
        }

        [HttpGet]
        [Route("tracuu2/searchTax")]
        [AllowAnonymous]
        public JObject SearchTax(string model)
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
                string originalString = ActionContext.Request.RequestUri.OriginalString;
                string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
                string folder = HttpContext.Current.Server.MapPath(path);
                string xml;
                string fileName;
                byte[] bytes = _tracuuService2.PrintInvoiceFromSbm(sobaomat, masothue, folder, type, out xml, out fileName);
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
                string masothue = model["masothue"].ToString();
                string sobaomat = model["sobaomat"].ToString();
                byte[] bytes = _tracuuService2.GetInvoiceXml(sobaomat, masothue);
                result = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attach")
                {
                    FileName = "invoice.xml"
                };
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
                result.Content.Headers.ContentLength = bytes.Length;
            }
            catch (Exception ex)
            {
                result = new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(ex.Message) };
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                result.Content.Headers.ContentLength = ex.Message.Length;
            }
            return result;
        }

        [HttpGet]
        [Authorize]
        [Route("tracuu2/getinfo")]
        [BaseAuthentication]
        public JObject GetInfoLogin()
        {
            string userName = "";
            string mst = "";
            ClaimsIdentity claimsIdentity = RequestContext.Principal.Identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                System.Collections.Generic.List<Claim> listClaim = claimsIdentity.Claims.ToList();
                userName = listClaim.FirstOrDefault(x => x.Type == "username")?.Value;
                mst = listClaim.FirstOrDefault(x => x.Type == "mst")?.Value;
                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(mst))
                {
                    JObject json = new JObject
                    {
                        {"status_code", 401 },
                        {"error", "Vui lòng đăng nhập để tiếp tục"}
                    };
                    return json;
                }
            }
            JObject result = _tracuuService2.GetInfoLogin(userName, mst);
            return result;
        }

        [HttpPost]
        [Route("tracuu2/showcert")]
        [AllowAnonymous]

        public JObject ShowCert(JObject model)
        {
            string id = model["id"].ToString();
            string xml = model["xml"].ToString();
            return _tracuuService2.ShowCert(id, xml);
        }

        [HttpPost]
        [Route("tracuu2/UploadInv")]
        [AllowAnonymous]
        public HttpResponseMessage UploadInv()
        {
            Stream streams = new MemoryStream();
            string type = "PDF";
            string fileName = null;
            try
            {
                HttpRequest httpRequest = HttpContext.Current.Request;
                if (httpRequest.Files.Count < 1)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Chưa chọn file hóa đơn");
                }
                foreach (string file in httpRequest.Files)
                {
                    HttpPostedFile postedFile = httpRequest.Files[file];
                    if (postedFile != null)
                    {
                        streams = postedFile.InputStream;
                        fileName = postedFile.FileName;
                    }
                }
                JObject json = new JObject();

                if (fileName != null && !fileName.EndsWith("zip"))
                {
                    json.Add("error", "File tải lên không đúng *.zip");
                    json.Add("status", "server");
                    return Request.CreateResponse(HttpStatusCode.BadRequest, json);
                }

                string xml = "";
                string repx = "";
                string key = "";
                ReportUtil.ExtracInvoice(streams, ref xml, ref repx, ref key);
                string xmlDecryp = EncodeXML.Decrypt(xml, key);
                string originalString = ActionContext.Request.RequestUri.OriginalString;
                string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
                string folder = HttpContext.Current.Server.MapPath(path);
                byte[] buffer = ReportUtil.InvoiceReport(xmlDecryp, repx, folder, "PDF");
                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(buffer) };
                if (type == "PDF")
                {
                    result.Content.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue("inline") { FileName = "Invoice.pdf" };
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                }
                result.Content.Headers.ContentLength = buffer.Length;
                return result;
            }
            catch (Exception ex)
            {
                JObject json = new JObject { { "error", ex.Message }, { "status", "server" } };
                return Request.CreateResponse(HttpStatusCode.BadRequest, json);
            }
        }


        [HttpPost]
        [Route("tracuu2/VeryfyXml")]
        [AllowAnonymous]
        public HttpResponseMessage VeryfyXml()
        {
            JObject json = new JObject();
            Stream streams = new MemoryStream();
            string fileName = null;
            try
            {
                HttpRequest httpRequest = HttpContext.Current.Request;
                if (httpRequest.Files.Count < 1)
                {
                    json.Add("error", "Chưa chọn file hóa đơn");
                    return Request.CreateResponse(HttpStatusCode.BadRequest, json);
                }
                foreach (string file in httpRequest.Files)
                {
                    HttpPostedFile postedFile = httpRequest.Files[file];
                    if (postedFile != null)
                    {
                        streams = postedFile.InputStream;
                        fileName = postedFile.FileName;
                    }
                }
                if (fileName != null && !fileName.EndsWith("zip"))
                {
                    json.Add("error", "File tải lên không đúng *.zip");
                    json.Add("status", "server");

                    return Request.CreateResponse(HttpStatusCode.BadRequest, json);
                }
                string xml = "";
                string repx = "";
                string key = "";
                ReportUtil.ExtracInvoice(streams, ref xml, ref repx, ref key);
                string xmlDecryp = EncodeXML.Decrypt(xml, key);

                JObject result = ReportUtil.VeryfyXml(xmlDecryp);
                return Request.CreateResponse(HttpStatusCode.BadRequest, result);
            }
            catch (Exception ex)
            {
                json.Add("error", ex.Message);
                return Request.CreateResponse(HttpStatusCode.BadRequest, json);
            }
        }


        [HttpPost]
        [Route("tracuu2/UploadInv2")]
        [AllowAnonymous]
        public HttpResponseMessage UploadInv(JObject model)
        {
            string type = "PDF";
            try
            {
                if (!model.ContainsKey("data"))
                {
                    JObject json = new JObject { { "error", "Chưa chọn file hóa đơn" }, { "status", "server" } };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, json);
                }
                byte[] bytes = Convert.FromBase64String(model["data"].ToString());
                Stream streams = new MemoryStream(bytes);
                string xml = "";
                string repx = "";
                string key = "";
                ReportUtil.ExtracInvoice(streams, ref xml, ref repx, ref key);
                string xmlDecryp = string.IsNullOrEmpty(key) ? xml : EncodeXML.Decrypt(xml, key);
                string originalString = ActionContext.Request.RequestUri.OriginalString;
                string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
                string folder = HttpContext.Current.Server.MapPath(path);
                byte[] buffer = ReportUtil.InvoiceReport(xmlDecryp, repx, folder, "PDF");
                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(buffer) };
                if (type == "PDF")
                {
                    result.Content.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue("inline") { FileName = "Invoice.pdf" };
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                }

                result.Content.Headers.ContentLength = buffer.Length;
                return result;
            }
            catch (Exception ex)
            {
                JObject json = new JObject { { "error", ex.Message }, { "status", "server" } };
                return Request.CreateResponse(HttpStatusCode.BadRequest, json);
            }
        }


        [HttpPost]
        [Route("tracuu2/VeryfyXml2")]
        [AllowAnonymous]
        public HttpResponseMessage VeryfyXml(JObject model)
        {
            JObject json = new JObject();
            try
            {
                if (!model.ContainsKey("data"))
                {
                    json = new JObject { { "error", "Chưa chọn file hóa đơn" }, { "status", "server" } };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, json);
                }
                byte[] bytes = Convert.FromBase64String(model["data"].ToString());
                Stream streams = new MemoryStream(bytes);
                string xml = "";
                string repx = "";
                string key = "";
                ReportUtil.ExtracInvoice(streams, ref xml, ref repx, ref key);
                string xmlDecryp = EncodeXML.Decrypt(xml, key);
                JObject result = ReportUtil.VeryfyXml(xmlDecryp);
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
