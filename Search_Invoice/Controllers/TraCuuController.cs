using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Search_Invoice.Util;
using Search_Invoice.Services;

namespace Search_Invoice.Controllers
{
    public class TraCuuController : Controller
    {
        [Authorize(Roles = "Admin, Editor")]
        public ActionResult UploadAPI()
        {
            return View();
        }
        // GET: TraCuu
        public ActionResult Upload()
        {
            return View();
        }
        //[HttpPost]
        public ActionResult UploadView(HttpPostedFileBase file, string chuoixacthuc)
        {
            if ((file == null) || (file.ContentLength <= 0))
            {
                return base.RedirectToAction("Upload");
            }
            string xml = "";
            string repx = "";
            string key = "";
            ReportUtil.ExtracInvoice(file.InputStream, ref xml, ref repx, ref key);
            string xmlDecryp = EncodeXML.Decrypt(xml, key);

            // byte[] buffer = null;
            string folder = base.Server.MapPath("~/Content/report/");
            byte[] buffer = ReportUtil.InvoiceReport(xmlDecryp, repx, folder, "PDF");
            base.Response.AppendHeader("Content-Disposition", "inline; filename=invoice.pdf");
            return base.File(buffer, "application/pdf");
        }
        public ContentResult Review()
        {
            string s = "";
            return Content(s, "application/json", System.Text.Encoding.UTF8);
        }

        //public ActionResult UploadView()
        //{
        //    return View();
        //}

    }
}