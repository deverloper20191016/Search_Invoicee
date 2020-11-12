using System.Web.Mvc;
using System.Web.Routing;

namespace Search_Invoice
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Quản lý người sử dụng",
                "quan-ly/nguoi-su-dung",
                new {controller = "NguoiSuDung", action = "Index", id = UrlParameter.Optional}
            );

            routes.MapRoute(
                "Quản lý quyền hạn",
                "quan-ly/quyen-han",
                new {controller = "NhomQuyen", action = "Index", id = UrlParameter.Optional}
            );

            routes.MapRoute(
                "hoa-don-dien-tu-la-gi",
                "hoa-don-dien-tu-la-gi",
                new {controller = "Home", action = "Hddtlagi", id = UrlParameter.Optional}
            );

            routes.MapRoute(
                "trang-chu",
                "trang-chu",
                new {controller = "PageHome", action = "PageHomeIndex", id = UrlParameter.Optional}
            );
            routes.MapRoute(
                "dang-nhap",
                "dang-nhap",
                new {controller = "Account", action = "Login", id = UrlParameter.Optional}
            );
            routes.MapRoute(
                "quan-ly",
                "quan-ly",
                new {controller = "Home", action = "Index", id = UrlParameter.Optional}
            );

            routes.MapRoute(
                "quan-ly-database",
                "quan-ly-database",
                new {controller = "Admin", action = "inv_admin", id = UrlParameter.Optional}
            );

            routes.MapRoute(
                "tim-kiem-khachhang",
                "tim-kiem-khachhang",
                new {controller = "Admin", action = "Search", id = UrlParameter.Optional}
            );

            routes.MapRoute(
                "tim-kiem-invoice",
                "tim-kiem-invoice",
                new {controller = "Customer", action = "Search_Invoice", id = UrlParameter.Optional}
            );

            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new {controller = "PageHome", action = "PageHomeIndex", id = UrlParameter.Optional}
            );
        }
    }
}