using apoc_api.common.Managers;
using Owin;
using System.Web.Http;

namespace PWARAbilityTool
{
    public class Startup
    {
        public static TimeTokenManager tokenManager { get; set; }
        public static IconService iconService { get; set; }

        // This method is required by Katana:
        public void Configuration(IAppBuilder app)
        {
            var webApiConfiguration = ConfigureWebApi();

            // Use the extension method provided by the WebApi.Owin library:
            app.UseWebApi(webApiConfiguration);

            tokenManager = new TimeTokenManager();
            iconService = new IconService();
        }

        private HttpConfiguration ConfigureWebApi()
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                "DefaultApi",
                "api/{controller}/{id}",
                new { id = RouteParameter.Optional });

            config.Routes.MapHttpRoute(
                "ItemSearch",
                "api/search",
                new { controller = "item", action = "search" });

            return config;
        }
    }
}