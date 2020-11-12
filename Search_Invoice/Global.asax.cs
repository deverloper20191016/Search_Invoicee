using Search_Invoice.Services;
using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.WebApi;
using System.Reflection;
using System.Web.Security;
using Autofac.Integration.Mvc;
using Search_Invoice.Controllers;
using Search_Invoice.Data;
using Search_Invoice.DAL;


namespace Search_Invoice
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            RegisterAutofacApi();
        }
        // check cookie đăng nhập
        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            try
            {
                var authCookie = Request.Cookies[CommonConstants.UserCookie];
                if (authCookie != null)
                {
                    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                    if (authTicket != null && !authTicket.Expired)
                    {
                        var roles = authTicket.UserData.Split(',');
                        HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(new FormsIdentity(authTicket), roles);
                    }
                }
                else
                {
                    FormsAuthentication.SignOut();
                }
            }
            catch (Exception)
            {
                FormsAuthentication.SignOut();
            }
        }
        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            //HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            {
                //HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
                HttpContext.Current.Response.AddHeader("Cache-Control", "no-cache");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Authorization, Accept");
                HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000");
                HttpContext.Current.Response.End();
            }
        }

        private void RegisterAutofacApi()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(CustomerController).Assembly).PropertiesAutowired();
            // Get your HttpConfiguration.
            var config = GlobalConfiguration.Configuration;

            // Register your Web API controllers.
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterType<InvoiceDbContext>().InstancePerRequest();
            builder.RegisterType<MemoryCacheManager>().As<ICacheManager>().Named<ICacheManager>("nop_cache_static").SingleInstance();
            builder.RegisterType<NopDbContext>().As<INopDbContext>().InstancePerLifetimeScope();
            builder.RegisterType<AccountService>().As<IAccountService>().InstancePerLifetimeScope();
            builder.RegisterType<InvoiceService>().As<IInvoiceService>().InstancePerLifetimeScope();
            builder.RegisterType<TracuuService>().As<ITracuuService>().InstancePerLifetimeScope();
            builder.RegisterType<NopDbContext2>().As<INopDbContext2>().InstancePerLifetimeScope();
            builder.RegisterType<TracuuService2>().As<ITracuuService2>().InstancePerLifetimeScope();

            builder.Register<IWebHelper>(c => new WebHelper(new HttpContextWrapper(HttpContext.Current))).InstancePerLifetimeScope();
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}
