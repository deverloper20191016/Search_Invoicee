using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Search_Invoice.Startup))]
namespace Search_Invoice
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
