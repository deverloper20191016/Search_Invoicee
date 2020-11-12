using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DevExpress.XtraPrinting.Native;
using Search_Invoice.Data.Domain;
using Search_Invoice.Models;
using Search_Invoice.Services;
using Search_Invoice.Util;

namespace Search_Invoice.Controllers
{
    [Authorize]
    public class NguoiSuDungController : Controller
    {
        private readonly IInvoiceService _invoiceService;

        public NguoiSuDungController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }
        // GET: NguoiSuDung
        public ActionResult Index()
        {
            var data = _invoiceService.GetNguoiSuDungs();
            return View(data);
        }


        // GET: NguoiSuDung/Create
        public ActionResult Create()
        {
            GetDropDownList();
            return View(new NguoiSuDung
            {
                MaQuyen = "TC"
            });
        }

        // POST: NguoiSuDung/Create
        [HttpPost]
        public ActionResult Create(NguoiSuDung nguoiSuDung)
        {
            GetDropDownList();
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(nguoiSuDung);
                }

                var check = _invoiceService.GetNguoiSuDungById(nguoiSuDung.TenTruyCap);
                if (check != null && !string.IsNullOrEmpty(check.TenTruyCap))
                {
                    ModelState.AddModelError("", $"Tên truy cập: {check.TenTruyCap} đã tồn tại");
                    return View(nguoiSuDung);
                }

                nguoiSuDung.MatKhau = CommonUtil.Md5Hash(nguoiSuDung.MatKhau);

                var result = _invoiceService.InsertNguoiSuDung(nguoiSuDung);
                if (result == null)
                {
                    ModelState.AddModelError("", "Không tạo được người sử dụng");
                    return View(nguoiSuDung);
                }
                // TODO: Add insert logic here

                return RedirectToAction("Index", "NguoiSuDung");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(nguoiSuDung);
            }
        }

        // GET: NguoiSuDung/Edit/5
        public ActionResult Edit(string id)
        {
            GetDropDownList();
            var data = _invoiceService.GetNguoiSuDungById(id);
            return View(data);
        }

        // POST: NguoiSuDung/Edit/5
        [HttpPost]
        public ActionResult Edit(NguoiSuDung nguoiSuDung)
        {
            GetDropDownList();
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(nguoiSuDung);
                }

                var result = _invoiceService.UpdateNguoiSuDung(nguoiSuDung);
                if (result == null)
                {
                    ModelState.AddModelError("", "Cập nhật không thành công");
                    return View(nguoiSuDung);
                }
                // TODO: Add update logic here

                return RedirectToAction("Index", "NguoiSuDung");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(nguoiSuDung);
            }
        }

        // GET: NguoiSuDung/Delete/5
        public JsonResult Delete(string id)
        {
            var result = _invoiceService.DeleteNguoiSuDung(id);
            return Json(result
                ? new
                {
                    status = "ok",
                    message = "Xóa thành công"
                }
                : new
                {
                    status = "error",
                    message = "Xóa thất bại"
                });
        }

        public void GetDropDownList()
        {
            System.Collections.Generic.IEnumerable<QuyenHan> result = _invoiceService.GetQuyenHans();
            ViewBag.MaQuyen = new SelectList(result, "MaQuyen", "TenQuyen");
        }

    }
}