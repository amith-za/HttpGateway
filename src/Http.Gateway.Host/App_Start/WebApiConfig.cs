using Http.Gateway;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace Http.Gateway
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            var moduleCatalog = new SyncedModuleCatalog();
            var registryCatalog = new SyncedRegistryCatalog();

            config.Routes.MapHttpRoute(
                name: "Gateway",
                routeTemplate: "{*path}",
                handler: HttpClientFactory.CreatePipeline
                (
                    innerHandler: new HttpClientHandler(), // will never get here if proxy is doing its job
                    handlers: new[] { new HttpGatewayHandler(moduleCatalog, registryCatalog) }
                ),
                defaults: new { path = RouteParameter.Optional },
                constraints: null
            );
        }
    }
}
