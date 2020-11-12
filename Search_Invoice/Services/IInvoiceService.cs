using System.Collections.Generic;
using Search_Invoice.Data.Domain;
using Search_Invoice.Models;

namespace Search_Invoice.Services
{
    public interface IInvoiceService
    {
        List<QuyenHan> GetQuyenHans();
        QuyenHan GetByMaQUyen(string maQuyen);
        QuyenHan InsertQuyenHan(QuyenHan quyenHanToCreate);
        QuyenHan UpdateQuyenHan(QuyenHan quyenHanToUpdate);
        bool DeleteQuyenHan(string id);

        List<NguoiSuDung> GetNguoiSuDungs();
        NguoiSuDung GetNguoiSuDungById(string id);
        NguoiSuDung InsertNguoiSuDung(NguoiSuDung nguoiSuDungToCreate);
        NguoiSuDung UpdateNguoiSuDung(NguoiSuDung nguoiSuDungToUpdate);
        bool DeleteNguoiSuDung(string id);
    }
}