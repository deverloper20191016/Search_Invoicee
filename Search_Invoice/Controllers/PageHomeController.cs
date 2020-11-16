using System.Web.Mvc;

namespace Search_Invoice.Controllers
{
    public class PageHomeController : Controller
    {
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