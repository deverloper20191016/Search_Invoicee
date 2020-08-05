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
using Search_Invoice.Data;

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

        public JObject GetInvoiceFromdateTodate(DateTime tu_ngay, DateTime den_ngay)
        {

            inv_user us = (inv_user)Session[CommonConstants.USER_SESSION];


            TracuuHDDTContext tracuu = new TracuuHDDTContext();

            var checkTraCuu = tracuu.inv_customer_banneds.FirstOrDefault(x =>
                x.mst.Replace("-", "").Equals(us.mst.Replace("-", "")) && x.type.Equals("KHOATRACUU") && x.is_unblock == false);

            if (checkTraCuu != null && !string.IsNullOrEmpty(checkTraCuu.mst))
            {
                return new JObject
                {
                    {
                        "error", "Quý khách đang bị khóa tra cứu. Vui lòng liên hệ admin để giải quyết !"
                    }
                };
            }

            CommonConnect cn = new CommonConnect();
            cn.setConnect(us.mst);



            string sql = "SELECT * FROM inv_InvoiceAuth WHERE inv_invoiceIssuedDate >= '" + tu_ngay.ToString("yyyy-MM-dd") + "' and inv_invoiceIssuedDate <= '" + den_ngay.ToString("yyyy-MM-dd") + "' AND ma_dt ='" + us.ma_dt + "' AND inv_InvoiceAuth_id IN (SELECT inv_InvoiceAuth_id FROM InvoiceXmlData) ORDER BY inv_invoiceNumber ASC";



            var mst = us.mst;
            if (!string.IsNullOrEmpty(mst))
            {
                if (mst.Equals("0107009894") || mst.Equals("0107009894001") || mst.Equals("0107009894-001"))
                {
                    var maDoiTuong = us.ma_dt;
                    var userName = us.username;

                    var doiTuong =
                        cn.ExecuteCmd($"SELECT TOP 1 * FROM dbo.dmdt WHERE ma_dt = '{maDoiTuong}' OR ma_dt = '{userName}'");

                    if (doiTuong.Rows.Count > 0)
                    {
                        var dt_me_id = doiTuong.Rows[0]["dt_me_id"].ToString();
                        var quyen_tracuu = doiTuong.Rows[0]["quyen_tracuu"].ToString();
                        if (!string.IsNullOrEmpty(quyen_tracuu))
                        {
                            if (quyen_tracuu.Equals("Tất cả"))
                            {
                                sql = $"SELECT * FROM inv_InvoiceAuth WHERE (inv_invoiceIssuedDate >= '{tu_ngay:yyyy-MM-dd}' AND inv_invoiceIssuedDate <= '{den_ngay:yyyy-MM-dd}') AND inv_InvoiceAuth_id IN (SELECT inv_InvoiceAuth_id FROM InvoiceXmlData) ";
                                sql +=
                                    $" AND ma_dt IN (SELECT ma_dt FROM dbo.dmdt WHERE dt_me_id = '{dt_me_id}') ";
                                sql += " ORDER BY inv_invoiceNumber ASC ";
                            }
                        }
                    }
                }
            }

            DataTable dt = cn.ExecuteCmd(sql);

            dt.Columns.Add("mst", typeof(string));
            dt.Columns.Add("inv_auth_id", typeof(string));
            dt.Columns.Add("total_amount_detail", typeof(decimal));

            var connectionString = cn.GetInvoiceDb().Database.Connection.ConnectionString;

            byte[] byt = System.Text.Encoding.UTF8.GetBytes(connectionString);
            var b = Convert.ToBase64String(byt);

            foreach (DataRow row in dt.Rows)
            {
                var id = row["inv_InvoiceAuth_id"].ToString();
                var tableDetail =
                    cn.ExecuteCmd(
                        $"SELECT SUM(inv_TotalAmount) AS total_amount FROM dbo.inv_InvoiceAuthDetail WHERE inv_InvoiceAuth_id = '{id}'");

                row.BeginEdit();
                if (tableDetail.Rows.Count > 0)
                {
                    if (!string.IsNullOrEmpty(tableDetail.Rows[0]["total_amount"].ToString()))
                    {
                        row["total_amount_detail"] = tableDetail.Rows[0]["total_amount"].ToString();
                    }
                }
                row["mst"] = us.mst;
                //row["a"] = connectionString;
                row["inv_auth_id"] = b;
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