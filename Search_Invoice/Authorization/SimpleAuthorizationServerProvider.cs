using Microsoft.Owin.Security.OAuth;
using Search_Invoice.Data;
using Search_Invoice.Util;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Search_Invoice.Authorization
{
    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });
            var data = await context.Request.ReadFormAsync();
            var param = data.Where(x => x.Key == "mst").Select(x => x.Value).FirstOrDefault();
            var userName = context.UserName.Replace("-", "");
            var mst = param[0].Replace("-", "");
            var passWord = context.Password;
            if (string.IsNullOrEmpty(mst) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(passWord))
            {
                context.SetError("error", "Nhập đủ thông tin đăng nhập");
                return;
            }
            var traCuu = new TracuuHDDTContext();
            var user = traCuu.inv_users.FirstOrDefault(x => x.username.Replace("-", "").Equals(userName.Replace("-", "")) & x.mst.Replace("-", "").Equals(mst.Replace("-", "")));
            if (user == null)
            {
                context.SetError("error_login", $"Tài khoản {context.UserName} không tồn tại");
                return;
            }
            PassCommand crypt = new PassCommand(user.password);
            if (!crypt.CheckPassword(passWord))
            {
                context.SetError("error_login", "Mật khẩu không chính xác");
                return;
            }
            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim("username", context.UserName));
            identity.AddClaim(new Claim("ma_dt", user.ma_dt));
            identity.AddClaim(new Claim("mst", mst));
            context.Validated(identity);
        }
        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }
            return Task.FromResult<object>(null);
        }
    }
}