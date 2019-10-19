using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Search_Invoice.Controllers
{
    public class HomeController : Controller
    {

        [Authorize(Roles = "Admin, Editor")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Index1()
        {
            return View();
        }
        [Authorize(Roles = "Admin, Editor")]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public PartialViewResult Header()
        {
            int SoThongBao = 0;

           
            return PartialView("Header", SoThongBao.ToString());
        }

        public PartialViewResult Sidebar()
        {
            string phanhes= "";
            return PartialView("Sidebar", phanhes);
        }
    }
}