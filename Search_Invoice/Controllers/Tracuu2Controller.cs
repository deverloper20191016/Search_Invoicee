using Newtonsoft.Json.Linq;
using Search_Invoice.Services;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Web.Http;
using Search_Invoice.Authorization;
using Search_Invoice.Data;

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
                bool inchuyendoi = model.ContainsKey("inchuyendoi");
                TracuuHDDTContext tracuu = new TracuuHDDTContext();
                var checkTraCuu = tracuu.inv_customer_banneds.FirstOrDefault(x =>
                    x.mst.Replace("-", "").Equals(masothue.Replace("-", "")) && x.type.Equals("KHOATRACUU") && x.is_unblock == false);
                if (checkTraCuu != null && !string.IsNullOrEmpty(checkTraCuu.mst))
                {
                    throw new Exception("Quý khách đang bị khóa tra cứu. Vui lòng liên hệ admin để giải quyết");
                }
                string originalString = ActionContext.Request.RequestUri.OriginalString;
                string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
                var folder = System.Web.HttpContext.Current.Server.MapPath(path);
                string fileName;
                byte[] bytes = _tracuuService2.PrintInvoiceFromSbm(sobaomat, masothue, folder, type, inchuyendoi, out fileName);
                result = new HttpResponseMessage(HttpStatusCode.OK) {Content = new ByteArrayContent(bytes)};
                if (type == "PDF")
                {
                    result.Content.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue("inline") {FileName = fileName };
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
                string masothue = model["masothue"].ToString();
                string sobaomat = model["sobaomat"].ToString();
                string originalString = ActionContext.Request.RequestUri.OriginalString;
                string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
                TracuuHDDTContext tracuu = new TracuuHDDTContext();
                var checkTraCuu = tracuu.inv_customer_banneds.FirstOrDefault(x =>
                    x.mst.Replace("-", "").Equals(masothue.Replace("-", "")) && x.type.Equals("KHOATRACUU") && x.is_unblock == false);
                if (checkTraCuu != null && !string.IsNullOrEmpty(checkTraCuu.mst))
                {
                    throw new Exception("Quý khách đang bị khóa tra cứu. Vui lòng liên hệ admin để giải quyết");
                }
                var folder = System.Web.HttpContext.Current.Server.MapPath(path);
                string fileName = "";
                string key = "";
                byte[] bytes = _tracuuService2.ExportZipFileXml(sobaomat, masothue, folder, ref fileName, ref key);
                result = new HttpResponseMessage(HttpStatusCode.OK) {Content = new ByteArrayContent(bytes)};
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attach");
                result.Content.Headers.ContentDisposition.FileName = fileName + ".zip";
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
                string masothue = model["masothue"].ToString();
                string sobaomat = model["sobaomat"].ToString();
                string originalString = ActionContext.Request.RequestUri.OriginalString;
                string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
                var folder = System.Web.HttpContext.Current.Server.MapPath(path);
                string fileName = "";
                string key = "";
                byte[] bytes = _tracuuService2.ExportZipFileXml(sobaomat, masothue, folder, ref fileName, ref key);
                result = new HttpResponseMessage(HttpStatusCode.OK) {Content = new ByteArrayContent(bytes)};
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attach");
                result.Content.Headers.ContentDisposition.FileName = fileName + ".zip";
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
                string masothue = model["masothue"].ToString();
                string sobaomat = model["sobaomat"].ToString();
                string originalString = ActionContext.Request.RequestUri.OriginalString;
                string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
                var folder = System.Web.HttpContext.Current.Server.MapPath(path);
                string fileName = "";
                byte[] bytes = _tracuuService2.PrintInvoiceFromSbm(sobaomat, masothue, folder, "pdf", out fileName);
                result = new HttpResponseMessage(HttpStatusCode.OK) {Content = new ByteArrayContent(bytes)};
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attach");
                result.Content.Headers.ContentDisposition.FileName = fileName + ".pdf";
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
        [Route("tracuu2/gethtml")]
        [AllowAnonymous]
        public JObject GetHtml(JObject model)
        {
            string originalString = ActionContext.Request.RequestUri.OriginalString;
            string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
            var folder = System.Web.HttpContext.Current.Server.MapPath(path);
            model.Add("folder", folder);
            return _tracuuService2.GetHtml(model);
        }

        [HttpGet]
        [Route("tracuu2/searchTax")]
        [AllowAnonymous]
        public JObject SearchTax(String model)
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
                TracuuHDDTContext tracuu = new TracuuHDDTContext();
                var checkTraCuu = tracuu.inv_customer_banneds.FirstOrDefault(x =>
                    x.mst.Replace("-", "").Equals(masothue) && x.type.Equals("KHOATRACUU") && x.is_unblock == false);
                if (checkTraCuu != null && !string.IsNullOrEmpty(checkTraCuu.mst))
                {
                    result.Add("error", $"Quý khách đang bị khóa tra cứu. Vui lòng liên hệ admin để giải quyết");
                    return result;
                }
                string originalString = ActionContext.Request.RequestUri.OriginalString;
                string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
                var folder = System.Web.HttpContext.Current.Server.MapPath(path);
                string xml;
                string fileName;
                byte[] bytes = _tracuuService2.PrintInvoiceFromSbm(sobaomat, masothue, folder, type, false, out xml, out fileName);
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

        [HttpPost]
        [Route("tracuu2/searchinvoice")]
        [Authorize]
        [BaseAuthentication]
        public JObject SearchInvoice(JObject model)
        {
            string userName;
            string mst;
            string maDt;
            var claimsIdentity = RequestContext.Principal.Identity as ClaimsIdentity;
            if (claimsIdentity != null) 
            {
                var listClaim = claimsIdentity.Claims.ToList();
                userName = listClaim.FirstOrDefault(x => x.Type == "username")?.Value;
                mst = listClaim.FirstOrDefault(x => x.Type == "mst")?.Value;
                maDt = listClaim.FirstOrDefault(x => x.Type == "ma_dt")?.Value;
                model.Add("user_name", userName);
                model.Add("mst", mst);
                model.Add("ma_dt", maDt);
            }
            return _tracuuService2.SearchInvoice(model);
        }

        [HttpGet]
        [Authorize]
        [Route("tracuu2/getinfo")]
        [BaseAuthentication]
        public JObject GetInfoLogin()
        {
            var userName = "";
            var mst = "";
            var claimsIdentity = RequestContext.Principal.Identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                var listClaim = claimsIdentity.Claims.ToList();
                userName = listClaim.FirstOrDefault(x => x.Type == "username")?.Value;
                mst = listClaim.FirstOrDefault(x => x.Type == "mst")?.Value;
                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(mst))
                {
                    var json = new JObject
                    {
                        {"status_code", 401 },
                        {"error", "Vui lòng đăng nhập để tiếp tục"}
                    };
                    return json;
                }
            }
            var result = _tracuuService2.GetInfoLogin(userName, mst);
            return result;
        }

        [Authorize]
        [BaseAuthentication]
        [Route("tracuu2/getlistinvoice")]
        [HttpPost]
        public JObject GetListInvoice(JObject model)
        {
            string userName;
            string mst;
            var claimsIdentity = RequestContext.Principal.Identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                var listClaim = claimsIdentity.Claims.ToList();
                userName = listClaim.FirstOrDefault(x => x.Type == "username")?.Value;
                mst = listClaim.FirstOrDefault(x => x.Type == "mst")?.Value;
                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(mst))
                {
                    var json = new JObject {{"error", "Vui lòng đăng nhập để tiếp tục"}};
                    return json;
                }
                model.Add("user_name", userName);
                model.Add("mst", mst);
            }
            return _tracuuService2.GetListInvoice(model);
        }

        [Authorize]
        [HttpPost]
        [BaseAuthentication]
        [Route("tracuu2/getlistinvoicetype")]
        public JObject GetListInvoiceType(JObject model)
        {
            string userName;
            string mst;
            var claimsIdentity = RequestContext.Principal.Identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                var listClaim = claimsIdentity.Claims.ToList();
                userName = listClaim.FirstOrDefault(x => x.Type == "username")?.Value;
                mst = listClaim.FirstOrDefault(x => x.Type == "mst")?.Value;
                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(mst))
                {
                    var json = new JObject {{"error", "Vui lòng đăng nhập để tiếp tục"}};
                    return json;
                }
                model.Add("user_name", userName);
                model.Add("mst", mst);
            }
            return _tracuuService2.GetListInvoiceType(model);
        }

        [HttpPost]
        [BaseAuthentication]
        [Route("tracuu2/search")]
        [Authorize]
        public JObject Search(JObject model)
        {
            string userName;
            string mst;
            var claimsIdentity = RequestContext.Principal.Identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                var listClaim = claimsIdentity.Claims.ToList();
                userName = listClaim.FirstOrDefault(x => x.Type == "username")?.Value;
                mst = listClaim.FirstOrDefault(x => x.Type == "mst")?.Value;
                model.Add("user_name", userName);
                model.Add("mst", mst);
            }
            return _tracuuService2.Search(model);
        }

        [HttpPost]
        [Route("tracuu2/showcert")]
        [AllowAnonymous]

        public JObject ShowCert(JObject model)
        {
            var id = model["id"].ToString();
            var xml = model["xml"].ToString();
            return _tracuuService2.ShowCert(id, xml);
        }
    }
}
