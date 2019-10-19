using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Search_Invoice.Data;
using Search_Invoice.DAL;

namespace Search_Invoice.Controllers
{
    [Authorize]
    public class BaseDataController : Controller
    {
        private TracuuHDDTContext context;

        public BaseDataController()
        {
            context = new TracuuHDDTContext();
        }

        public TracuuHDDTContext DbContext
        {
            get { return this.context; }
        }

    }
    public class AuthorizeFilter : ActionFilterAttribute
    {
        public string MaChucNang { get; set; }
        public string Action { get; set; }

        //protected
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            User db = new User();

            string UserName = filterContext.HttpContext.User.Identity.Name;

            if (UserName.ToUpper() == "ADMINISTRATOR")
            {
                base.OnActionExecuting(filterContext);
                return;
            }
            if(MaChucNang == "Minvoice")
            {
                if(UserName != "truyennv")
                {
                    filterContext.Result = new RedirectResult("/Admin/AccessDenied/");
                }
            }
            if (MaChucNang == "TKMinvoice")
            {
                if (UserName != "admin" && UserName != "truyennv")
                {
                    filterContext.Result = new RedirectResult("/Admin/AccessDenied/");
                }
            }
            //  NguoiSuDung nsd = db.NguoiSuDungs.Where(c => c.TenTruyCap == UserName).FirstOrDefault<NguoiSuDung>();
            //CtPhanQuyenCn ct = db.CtPhanQuyenCns.Where(c => c.MaQuyen == nsd.MaQuyen && c.MaChucNang == MaChucNang).FirstOrDefault<CtPhanQuyenCn>();

                //if (UserName == "ADMINISTRATOR" || ct != null)
                //{
                //    if (UserName == "ADMINISTRATOR")
                //    {
                //        base.OnActionExecuting(filterContext);
                //    }
                //    else
                //    {
                //        if (Action == null)
                //        {
                //            base.OnActionExecuting(filterContext);
                //        }
                //        else
                //        {
                //            bool cmd = false;

                //            if (Action == "Insert")
                //            {
                //                cmd = ct.Them == true ? true : false;
                //            }
                //            else if (Action == "Update")
                //            {
                //                cmd = ct.Sua == true ? true : false;
                //            }
                //            else if (Action == "Delete")
                //            {
                //                cmd = ct.Xoa == true ? true : false;
                //            }

                //            if (cmd)
                //            {
                //                base.OnActionExecuting(filterContext);
                //            }
                //            else
                //            {
                //                filterContext.Result = new RedirectResult("/Hethong/AccessDenied/" + Action);
                //            }
                //        }
                //}
                //}
                //else
                //{

                //    filterContext.Result = new RedirectResult("/Hethong/AccessDenied/403");
                //}
        }
        //protected
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
        }
    }
}