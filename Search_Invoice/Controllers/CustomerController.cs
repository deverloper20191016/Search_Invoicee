using System;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using Search_Invoice.Services;

namespace Search_Invoice.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ITracuuService2 _tracuuService2;
        private readonly IWebHelper _webHelper;

        public CustomerController(IAccountService accountService, IWebHelper webHelper, ITracuuService2 tracuuService2)
        {
            _accountService = accountService;
            _webHelper = webHelper;
            _tracuuService2 = tracuuService2;
        }

        // GET: Customer
        public ActionResult Search_Invoice()
        {
            var userName = _webHelper.GetUser();
            var loginResult = _accountService.GetInfoByName(userName);
            if (loginResult == null)
            {
                return RedirectToAction("PageHomeIndex", "PageHome");
            }
            return View();
        }

        public JObject GetInvoiceFromdateTodate(DateTime tu_ngay, DateTime den_ngay)
        {
            var userName = _webHelper.GetUser();
            var tuNgay = tu_ngay.ToString("yyyy-MM-dd");
            var denNgay = den_ngay.ToString("yyyy-MM-dd");
            var result = _tracuuService2.SearchDataByDate(tuNgay, denNgay, userName);
            return result;
        }

        public ActionResult _Header()
        {
            var userName = _webHelper.GetUser();
            var loginResult = _accountService.GetInfoByName(userName);
           
            return PartialView("_Header", loginResult);
        }
    }
}