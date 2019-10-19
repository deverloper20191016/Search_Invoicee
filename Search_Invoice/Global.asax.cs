using Search_Invoice.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.WebApi;
using System.Reflection;
using System.Web.Security;
using Microsoft.Owin;
using System.Security.Principal;
using Microsoft.AspNet.Identity.Owin;


namespace Search_Invoice
{
    public class MvcApplication : System.Web.HttpApplication
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
            var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (authTicket != null && !authTicket.Expired)
                {
                    var roles = authTicket.UserData.Split(',');
                    HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(new FormsIdentity(authTicket), roles);
                }
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

            //var newCulture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            //newCulture.NumberFormat.NumberDecimalSeparator = ",";
            //newCulture.NumberFormat.NumberGroupSeparator = ".";

            //System.Threading.Thread.CurrentThread.CurrentCulture = newCulture;
            //System.Threading.Thread.CurrentThread.CurrentUICulture = newCulture;
        }

        private void RegisterAutofacApi()
        {
            var builder = new ContainerBuilder();

            // Get your HttpConfiguration.
            var config = GlobalConfiguration.Configuration;

            // Register your Web API controllers.
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            //builder.Register(ctx => HttpContext.Current.GetOwinContext()).As<IOwinContext>();
            //builder.Register(ctx => HttpContext.Current.User.Identity).As<IIdentity>().InstancePerLifetimeScope();
            //builder.Register(ctx => HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>()).As<ApplicationUserManager>().InstancePerLifetimeScope();
            //builder.Register(c => BundleTable.Bundles).As<BundleCollection>().InstancePerLifetimeScope();
            //builder.Register(c => RouteTable.Routes).As<RouteCollection>().InstancePerLifetimeScope();

            builder.RegisterType<MemoryCacheManager>().As<ICacheManager>().Named<ICacheManager>("nop_cache_static").SingleInstance();
            builder.Register<IWebHelper>(c => new WebHelper(new HttpContextWrapper(HttpContext.Current) as HttpContextBase)).InstancePerLifetimeScope();
            builder.RegisterType<NopDbContext>().As<INopDbContext>().InstancePerLifetimeScope();
            builder.RegisterType<TracuuService>().As<ITracuuService>().InstancePerLifetimeScope();
            builder.RegisterType<NopDbContext2>().As<INopDbContext2>().InstancePerLifetimeScope();
            builder.RegisterType<TracuuService2>().As<ITracuuService2>().InstancePerLifetimeScope();

            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);



        }
    }
}
