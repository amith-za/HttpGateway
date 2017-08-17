using Http.Gateway.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Http.Gateway.Modules;

namespace Http.Gateway
{
    public class RequestPipleline
    {
        public List<RouteInfo> HandledRoutes { get; private set; }

        public List<Module> PipelineModules { get; private set; }

        internal RequestPipleline(List<RouteInfo> handledRoutes, List<Module> modules)
        {
            HandledRoutes = handledRoutes;
            PipelineModules = modules;
            WireupPipeline();
        }

        private void WireupPipeline()
        {
            for (int i = 0; i < PipelineModules.Count - 1; i++)
            {
                PipelineModules[i].InnerHandler = PipelineModules[i + 1];
            }
        }

        public Task<HttpResponseMessage> Handle(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return PipelineModules[0].Handle(request, cancellationToken);
        }

        internal void Append(Module dispatcher)
        {
            PipelineModules.Add(dispatcher);
            WireupPipeline();
        }

        public bool CanHandle(HttpRequestMessage request)
        {
            var normalizedPath = request.NormalizedPath();

            var httpMethod = request.Method.Method;

            foreach (var route in HandledRoutes)
            {
                if (route.Match(httpMethod, normalizedPath))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
