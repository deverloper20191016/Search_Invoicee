using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using Search_Invoice.Data.Domain;
using Search_Invoice.DAL;
using Search_Invoice.Services;
using Search_Invoice.Util;
using System.Data;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;

namespace Search_Invoice.Controllers
{
    public class CustomerController : BaseDataController
    {
        //private ITracuuService2 _tracuuService2;

        //public CustomerController(ITracuuService2 tracuuService2)
        //{
        //    this._tracuuService2 = tracuuService2;
        //}
        // GET: Customer
        public ActionResult Search_Invoice()
        {
            return View();
        }

        public JObject GetInvoiceFromdateTodate(DateTime tu_ngay ,DateTime  den_ngay)
        {
       
            inv_user us = (inv_user)Session[CommonConstants.USER_SESSION];
;
            CommonConnect cn = new CommonConnect();
            cn.setConnect(us.mst);
            string sql = "SELECT * FROM inv_InvoiceAuth WHERE inv_invoiceIssuedDate >= '" + tu_ngay.ToString("yyyy-MM-dd") + "' and inv_invoiceIssuedDate <= '" + den_ngay.ToString("yyyy-MM-dd") + "' AND ma_dt ='" + us.ma_dt + "' AND inv_InvoiceAuth_id IN (SELECT inv_InvoiceAuth_id FROM InvoiceXmlData) ORDER BY inv_invoiceNumber ASC";
            DataTable dt = cn.ExecuteCmd(sql);
            dt.Columns.Add("mst", typeof(string));
            dt.Columns.Add("a", typeof(string));

            var connectionString = cn.GetInvoiceDb().Database.Connection.ConnectionString;

            foreach (DataRow row in dt.Rows)
            {
                row.BeginEdit();
                row["mst"] = us.mst;
                row["a"] = connectionString;
                row.EndEdit();
            }

            JObject result = new JObject();
            if (dt.Rows.Count > 0)
            {
                JArray jar = JArray.FromObject(dt);
                result.Add("data", jar);
            }
            else
            {
                result.Add("error", "Không tìm thấy dữ liệu.");
            }
            return result;
            //return Json(result, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //[Route("Tracuu2/PrintInvoice")]
        //[AllowAnonymous]
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
                string path = Server.MapPath("~/Content/report/");
                //string originalString = this.ActionContext.Request.RequestUri.OriginalString;
                //string path = originalString.StartsWith("/api") ? "~/api/Content/report/" : "~/Content/report/";
                //string path = "~/Content/report/";
                var folder = System.Web.HttpContext.Current.Server.MapPath(path);

                byte[] bytes = null;
                    //_tracuuService2.PrintInvoiceFromSBM(sobaomat, masothue, folder, type);

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

        public JsonResult SaveEmployeeRecord(string id, string name)
        {
            string this_id = id;
            string this_name = name;
            // do here some operation  
            return Json(new { id = this_id, name = this_name }, JsonRequestBehavior.AllowGet);
        }
    }
}