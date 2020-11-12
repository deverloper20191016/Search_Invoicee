using System;
using System.Web.Mvc;
using Search_Invoice.Data.Domain;
using Search_Invoice.Models;
using Search_Invoice.Services;

namespace Search_Invoice.Controllers
{
    [Authorize]
    public class NhomQuyenController : Controller
    {
        private readonly IInvoiceService _invoiceService;

        public NhomQuyenController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        // GET: NhomQuyen
        public ActionResult Index()
        {
            var data = _invoiceService.GetQuyenHans();
            return View(data);
        }

        // GET: NhomQuyen/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: NhomQuyen/Create
        [HttpPost]
        public ActionResult Create(QuyenHan quyenHan)
        {
            try
            {
                if (!ModelState.IsValid) return View(quyenHan);

                var check = _invoiceService.GetByMaQUyen(quyenHan.MaQuyen);
                if (check != null && !string.IsNullOrEmpty(check.MaQuyen))
                {
                    ModelState.AddModelError("", $"Mã quyền: {check.MaQuyen} đã tồn tại");
                    return View(quyenHan);
                }

                var result = _invoiceService.InsertQuyenHan(quyenHan);
                if (result == null)
                {
                    ModelState.AddModelError("", "Không tạo được danh mục quyền hạn");
                    return View(quyenHan);
                }
                // TODO: Add insert logic here

                return RedirectToAction("Index", "NhomQuyen");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(quyenHan);
            }
        }

        // GET: NhomQuyen/Edit/5
        public ActionResult Edit(string id)
        {
            var result = _invoiceService.GetByMaQUyen(id);
            return View(result);
        }

        // POST: NhomQuyen/Edit/5
        [HttpPost]
        public ActionResult Edit(QuyenHan quyenHan)
        {
            try
            {
                if (!ModelState.IsValid) return View(quyenHan);

                var result = _invoiceService.UpdateQuyenHan(quyenHan);
                if (result == null) ModelState.AddModelError("", "Cập nhật thất bại");
                // TODO: Add update logic here

                return RedirectToAction("Index", "NhomQuyen");
            }
            catch
            {
                return View();
            }
        }

        // GET: NhomQuyen/Delete/5
        public JsonResult Delete(string id)
        {
            var result = _invoiceService.DeleteQuyenHan(id);
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
    }
}