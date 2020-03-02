using System.Web.Http;

namespace TrboPortal
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // config.EnableSwagger(c => c.SingleApiVersion("v13", "A title for your API")).EnableSwaggerUi();
        }
    }
}
