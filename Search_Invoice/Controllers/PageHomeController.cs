using Newtonsoft.Json.Linq;
using Search_Invoice.Data;
using Search_Invoice.Data.Domain;
using Search_Invoice.Models;
using Search_Invoice.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Search_Invoice.Controllers
{
    public class PageHomeController : Controller
    {
        private INopDbContext _nopDbContext;
       
        // GET: PageHome

        [AllowAnonymous]
        public ActionResult PageHomeIndex()
        {
            return View();
        }
        [AllowAnonymous]
        public PartialViewResult PageHomeHeader()
        {
            return PartialView("PageHomeHeader");
        }

    }
}