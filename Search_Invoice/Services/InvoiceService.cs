using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Search_Invoice.Data;
using Search_Invoice.Data.Domain;

namespace Search_Invoice.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly InvoiceDbContext _dbContext;
        private readonly IWebHelper _webHelper;

        public InvoiceService(InvoiceDbContext dbContext, IWebHelper webHelper)
        {
            _dbContext = dbContext;
            _webHelper = webHelper;
        }
        public List<QuyenHan> GetQuyenHans()
        {
            var data = _dbContext.QuyenHans.ToList();
            return data;
        }

        public QuyenHan GetByMaQUyen(string maQuyen)
        {
            var data = _dbContext.QuyenHans.SingleOrDefault(x => x.MaQuyen.Equals(maQuyen));
            return data;
        }

        public QuyenHan InsertQuyenHan(QuyenHan quyenHanToCreate)
        {

            var paramester = new Dictionary<string, object>
            {
                { "MaQuyen",quyenHanToCreate.MaQuyen },
                { "TenQuyen",quyenHanToCreate.TenQuyen }
            };
            var sql = "INSERT INTO dbo.DmQuyenHan(MaQuyen, TenQuyen) VALUES(@MaQuyen, @TenQuyen)";
            var result = _dbContext.ExecuteNoneQuery(sql, paramester);
            if (result > 0)
            {
                return quyenHanToCreate;
            }

            return null;
        }

        public QuyenHan UpdateQuyenHan(QuyenHan quyenHanToUpdate)
        {
            var paramester = new Dictionary<string, object>
            {
                { "MaQuyen",quyenHanToUpdate.MaQuyen },
                { "TenQuyen",quyenHanToUpdate.TenQuyen }
            };
            var sql = "UPDATE dbo.DmQuyenHan SET TenQuyen = @TenQuyen WHERE MaQuyen = @MaQuyen";
            var result = _dbContext.ExecuteNoneQuery(sql, paramester);
            if (result > 0)
            {
                return quyenHanToUpdate;
            }

            return null;
        }

        public bool DeleteQuyenHan(string id)
        {
            var sql = $"DELETE dbo.DmQuyenHan WHERE MaQuyen = '{id}'";
            var result = _dbContext.ExecuteNoneQuery(sql);
            return result > 0;
        }

        public List<NguoiSuDung> GetNguoiSuDungs()
        {
            var userName = _webHelper.GetUser();
            var data = _dbContext.ExecuteCmd($"SELECT a.*, b.TenQuyen FROM dbo.NguoiSuDung AS a LEFT JOIN dbo.DmQuyenHan AS b ON b.MaQuyen = a.MaQuyen WHERE a.TenTruyCap <> '{userName}'");
            var result = new List<NguoiSuDung>();
            if (data.Rows.Count > 0)
            {
                result.AddRange(from DataRow row in data.Rows
                                select new NguoiSuDung
                                {
                                    TenTruyCap = row["TenTruyCap"].ToString(),
                                    TenNguoiDung = row["TenNguoiDung"].ToString(),
                                    MaQuyen = row["MaQuyen"].ToString(),
                                    TenQuyen = row["TenQuyen"].ToString()
                                });
            }
            return result;
        }

        public NguoiSuDung GetNguoiSuDungById(string id)
        {
            var data = _dbContext.ExecuteCmd($"SELECT TOP 1 * FROM dbo.NguoiSuDung WHERE TenTruyCap = '{id}'");
            var result = new List<NguoiSuDung>();
            if (data.Rows.Count > 0)
            {
                result.AddRange(from DataRow row in data.Rows
                                select new NguoiSuDung
                                {
                                    TenTruyCap = row["TenTruyCap"].ToString(),
                                    TenNguoiDung = row["TenNguoiDung"].ToString(),
                                    MaQuyen = row["MaQuyen"].ToString(),
                                });
            }

            return result.Count == 1 ? result[0] : null;
        }

        public NguoiSuDung InsertNguoiSuDung(NguoiSuDung nguoiSuDungToCreate)
        {
            var paramester = new Dictionary<string, object>
            {
                { "id", Guid.NewGuid() },
                { "TenTruyCap",nguoiSuDungToCreate.TenTruyCap },
                { "MatKhau",nguoiSuDungToCreate.MatKhau },
                { "TenNguoiDung",nguoiSuDungToCreate.TenNguoiDung },
                { "MaQuyen",nguoiSuDungToCreate.MaQuyen },
            };
            var sql = "INSERT INTO dbo.NguoiSuDung(id, TenTruyCap, MatKhau, TenNguoiDung, MaQuyen) VALUES(@id, @TenTruyCap, @MatKhau, @TenNguoiDung, @MaQuyen)";
            var result = _dbContext.ExecuteNoneQuery(sql, paramester);
            if (result > 0)
            {
                return nguoiSuDungToCreate;
            }

            return null;
        }

        public NguoiSuDung UpdateNguoiSuDung(NguoiSuDung nguoiSuDungToUpdate)
        {
            var paramester = new Dictionary<string, object>
            {
                { "TenTruyCap",nguoiSuDungToUpdate.TenTruyCap },
                { "TenNguoiDung",nguoiSuDungToUpdate.TenNguoiDung },
                { "MaQuyen",nguoiSuDungToUpdate.MaQuyen },
            };
            var sql = "UPDATE dbo.NguoiSuDung SET TenNguoiDung = @TenNguoiDung, MaQuyen = @MaQuyen WHERE TenTruyCap = @TenTruyCap";
            var result = _dbContext.ExecuteNoneQuery(sql, paramester);
            if (result > 0)
            {
                return nguoiSuDungToUpdate;
            }

            return null;
        }

        public bool DeleteNguoiSuDung(string id)
        {
            var sql = $"DELETE dbo.NguoiSuDung WHERE TenTruyCap = '{id}' ";
            var result = _dbContext.ExecuteNoneQuery(sql);
            return result > 0;
        }
    }
}