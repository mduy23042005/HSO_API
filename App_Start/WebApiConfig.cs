using System.Net.Http.Headers;
using System.Web.Http;

public static class WebApiConfig
{
    public static void Register(HttpConfiguration config)
    {
        // Bật attribute routing
        config.MapHttpAttributeRoutes();

        // Route mặc định
        config.Routes.MapHttpRoute(
            name: "HSO_WebAPI",
            routeTemplate: "api/{controller}/{id}",
            defaults: new { id = RouteParameter.Optional }
        );

        // Chỉ trả JSON, không trả XML
        config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
        config.Formatters.JsonFormatter.SupportedMediaTypes
            .Add(new MediaTypeHeaderValue("application/json"));
    }
}
