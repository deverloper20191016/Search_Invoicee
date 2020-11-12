using System.Linq;
using Newtonsoft.Json.Linq;
using Search_Invoice.Data;
using Search_Invoice.Data.Domain;
using Search_Invoice.Models;
using Search_Invoice.Util;

namespace Search_Invoice.Services
{
    public class AccountService : IAccountService
    {
        private readonly InvoiceDbContext _dbContext;
        public AccountService(InvoiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public LoginResult Login(string userName, string passWord)
        {
            var nguoiSuDung = _dbContext.NguoiSuDungs.SingleOrDefault(x => x.TenTruyCap.Equals(userName));
            var loginResult = new LoginResult();
            if (nguoiSuDung == null)
            {
                loginResult.Loi = "Tài khoản không tồn tại";
            }
            else
            {
                if (CommonUtil.Md5Hash(passWord).Equals(nguoiSuDung.MatKhau))
                {
                    loginResult.TenTruyCap = nguoiSuDung.TenTruyCap;
                    loginResult.TenNguoiDung = nguoiSuDung.TenNguoiDung;
                    loginResult.MaQuyen = nguoiSuDung.MaQuyen;
                }
                else
                {
                    loginResult.Loi = "Mật khẩu không chính xác";
                }
            }
            return loginResult;
        }

        public NguoiSuDung GetInfoByName(string tenTruyCap)
        {
            var data = _dbContext.NguoiSuDungs.SingleOrDefault(x => x.TenTruyCap.Equals(tenTruyCap));
            return data;
        }

        public JObject ChangePassword(string tenTruyCap, string matKhauMoi)
        {
            var json = new JObject();
            try
            {
                var data = _dbContext.NguoiSuDungs.SingleOrDefault(x => x.TenTruyCap.Equals(tenTruyCap));
                if (data== null)
                {
                    json.Add("error", "Không tìm thấy tên truy cập");
                    return json;
                }

                if (string.IsNullOrEmpty(matKhauMoi))
                {
                    json.Add("error", "Vui lòng nhập mật khẩu");
                    return json;
                }

                var matKhauMoiHash = CommonUtil.Md5Hash(matKhauMoi);
                var sql = $"UPDATE dbo.NguoiSuDung SET MatKhau = '{matKhauMoiHash}' WHERE TenTruyCap = '{tenTruyCap}' ";
                _dbContext.ExecuteNoneQuery(sql);
                json.Add("ok", "Cập nhật thành công");
                return json;
            }
            catch (System.Exception ex)
            {
                json.Add("error", ex.Message);
                return json;
            }

        }
    }
}