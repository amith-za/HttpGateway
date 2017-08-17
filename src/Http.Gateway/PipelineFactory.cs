using Http.Gateway.Configuration;
using Http.Gateway.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway
{
    public class PipelineFactory
    {
        public IModuleCatalog ModuleCatalog { get; private set; }

        public IRegistryCatalog RegistryCatalog { get; private set; }

        public PipelineFactory(IModuleCatalog moduleCatalog, IRegistryCatalog registryCatalog)
        {
            ModuleCatalog = moduleCatalog;
            RegistryCatalog = registryCatalog;
        }

        public RequestPipleline CreatePipeline(RoutingDefinition routingDefinition)
        {
            var globalmodules = ModuleCatalog.GetGlobalModules();

            var routeModules = new List<Module>(globalmodules);

            foreach (var routeModule in routingDefinition.RouteModules)
            {
                if (!string.Equals(routeModule.Id, "none", StringComparison.InvariantCultureIgnoreCase))
                {
                    var module = ModuleCatalog.CreateById(routeModule.Id, routeModule.Configuration);
                    if (module != null)
                    {
                        routeModules.Add(module);
                    }
                }
            }

            var requestPipeline = new RequestPipleline(routingDefinition.Routes, routeModules);

            var dispatcher = new RequestDispatchModule(routingDefinition, RegistryCatalog);

            requestPipeline.Append(dispatcher);

            return requestPipeline;
        }
    }
}
