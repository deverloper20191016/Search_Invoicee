using Newtonsoft.Json.Linq;
using Search_Invoice.Data.Domain;
using Search_Invoice.Models;

namespace Search_Invoice.Services
{
    public interface IAccountService
    {
        LoginResult Login(string userName, string passWord);

        NguoiSuDung GetInfoByName(string tenTruyCap);

        JObject ChangePassword(string tenTruyCap, string matKhauMoi);
    }
}
