using Search_Invoice.Data.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Search_Invoice.Data;
using Search_Invoice.Models;
using Search_Invoice.Extensions;
using Newtonsoft.Json.Linq;

namespace Search_Invoice.Controllers
{
    //[Authorize]
    public class AdminController : BaseDataController
    {
        private TracuuHDDTContext _tracuuHDDTContext;

        public AdminController()
        {
            _tracuuHDDTContext = new TracuuHDDTContext();
        }
        [AuthorizeFilter(MaChucNang = "TKMinvoice", Action = "View")]
        public ActionResult Search()
        {
            return View();
        }
        [AuthorizeFilter(MaChucNang = "TKMinvoice", Action = "View")]
        public JObject Search_Tax(string mst)
        {
            JObject json = new JObject();
            try
            {
                inv_admin admin = _tracuuHDDTContext.Inv_admin.Where(c => c.MST == mst).FirstOrDefault<inv_admin>();
                if(admin == null)
                {
                    json.Add("error", "Không tồn tại MST : " + mst);
                    return json;
                }
                var json1 = Newtonsoft.Json.JsonConvert.SerializeObject(admin);
                json = JObject.Parse(json1);
            }
            catch(Exception ex)
            {
                json.Add("error", ex.Message);
            }
            return json;
        }
        // page không có quyền
        public ActionResult AccessDenied ()
        {
            return View();
        }

       // GET: Admin
       // [Authorize(Roles = "Admin")]
       //[HttpPost]
       [AuthorizeFilter(MaChucNang = "Minvoice", Action = "Insert")]
        public ActionResult inv_admin()
        {
            return View();
        }
        //[Authorize(Roles = "Admin")]
        public JsonResult DSinv_admin(KendoPostData data)
        {
          
            string filter = "";
            int total = 0;
            int skip = data.Skip;
            int take = data.Take;

            if (data.Filter != null)
            {
                if (data.Filter.Filters != null)
                {
                    filter = " WHERE " + HtmlHelperExtensions.KendoFilter(data.Filter);
                }
            }

            List<inv_admin> lst = null;

            if (data.PageSize == 0)
            {
                lst = this._tracuuHDDTContext.Inv_admin
                                .SqlQuery("SELECT * FROM inv_admin " + filter + " ORDER BY MST")
                                .OrderBy(c => c.MST)
                                .ToList<inv_admin>();
            }
            else
            {
                total = this._tracuuHDDTContext.Inv_admin
                                          .SqlQuery("SELECT * FROM inv_admin " + filter)
                                          .Count();

                lst = this._tracuuHDDTContext.Inv_admin
                                    .SqlQuery("SELECT * FROM inv_admin " + filter + " ORDER BY MST")
                                    .OrderBy(c => c.MST)
                                    .Skip(skip)
                                    .Take(take)
                                    .ToList<inv_admin>();
            }

            KendoData<inv_admin> dataSource = new KendoData<inv_admin>();
            dataSource.data = lst;
            dataSource.total = total;

            return Json(dataSource, "application/json", JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            inv_admin ad = new inv_admin();
            ad.inv_admin_id = Guid.NewGuid();
            return View(ad);
        }
        [Authorize(Roles = "Admin")]
        public JsonResult JsonCreate(inv_admin model)
        {
            ResultJson<inv_admin> json = new ResultJson<inv_admin>();
            json.item = model;
            json.msg = "failure";

            inv_admin list = this._tracuuHDDTContext.Inv_admin.Where(c => c.MST == model.MST).FirstOrDefault<inv_admin>();

            inv_admin qh = this._tracuuHDDTContext.Inv_admin.Find(model.inv_admin_id);

            if (list != null)
            {
                json.description = "Mã số thuế này đã có !";
                return Json(json, "application/json");
            }
            else
            {
                if (qh != null)
                {
                    json.description = "Mã này đã có !";
                    return Json(json, "application/json");
                }

                if (ModelState.IsValid)
                {
                    this._tracuuHDDTContext.Inv_admin.Add(model);
                    this._tracuuHDDTContext.SaveChanges();

                    json.msg = "success";

                    return Json(json, "application/json");
                }
            }
            return Json(json, "application/json");
        }
        [Authorize(Roles = "Admin")]
        public JsonResult Delete(Guid inv_admin_id)
        {
            ResultJson<inv_admin> json = new ResultJson<inv_admin>();

            inv_admin item = this._tracuuHDDTContext.Inv_admin.Find(inv_admin_id);

            try
            {
                this._tracuuHDDTContext.Inv_admin.Remove(item);
                this._tracuuHDDTContext.SaveChanges();

                json.msg = "success";
            }
            catch (Exception ex)
            {
                json.msg = ex.Message;
            }

            return Json(json, "application/json");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Update(Guid id)
        {
            inv_admin ds = this._tracuuHDDTContext.Inv_admin.Where(c => c.inv_admin_id == id).FirstOrDefault<inv_admin>();
            return View(ds);
        }
        [Authorize(Roles = "Admin")]
        public JsonResult JsonUpdate(inv_admin model)
        {
            ResultJson<inv_admin> json = new ResultJson<inv_admin>();
            json.item = model;
            json.msg = "failure";

            List<inv_admin> lst = this._tracuuHDDTContext.Inv_admin.SqlQuery("SELECT * FROM inv_admin WHERE inv_admin_id !='" + model.inv_admin_id + "'").ToList<inv_admin>();

            for (int i = 0; i < lst.Count; i++)
            {
                if (lst[i].MST == model.MST)
                {
                    json.description = "Mã số thuế này đã có ";
                    return Json(json, "application/json");
                }
            }

            try
            {
                if (ModelState.IsValid)
                {
                    this._tracuuHDDTContext.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    this._tracuuHDDTContext.SaveChanges();
                    json.msg = "success";
                }
            }
            catch (Exception ex)
            {
                json.description = ex.Message;
            }

            return Json(json, "application/json");
        }
    }
}