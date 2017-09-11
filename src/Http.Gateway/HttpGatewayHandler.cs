using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Http.Gateway.Configuration.Dsl;
using Http.Gateway.Configuration;
using RethinkDb.Driver;

namespace Http.Gateway
{
    public class HttpGatewayHandler : DelegatingHandler
    {
        private static SyncedServiceScripts _serviceScripts;

        public IModuleCatalog ModuleCatalog { get; private set; }

        public IRegistryCatalog RegistryCatalog { get; private set; }

        static HttpGatewayHandler()
        {
            _serviceScripts = new SyncedServiceScripts(RethinkDB.R.Connection().Hostname(Settings.RethinkDatabaseHost));
            _serviceScripts.StartSync();
        }

        public HttpGatewayHandler(IModuleCatalog moduleCatalog, IRegistryCatalog registryCatalog)
        {
            if (moduleCatalog == null)
            {
                throw new ArgumentNullException(nameof(moduleCatalog));
            }

            if (registryCatalog == null)
            {
                throw new ArgumentNullException(nameof(registryCatalog));
            }

            ModuleCatalog = moduleCatalog;
            RegistryCatalog = registryCatalog;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var pipeline = GetPipeline(request);

            if (pipeline != null)
            {
                request.Properties.Add(RequestRoutingProperties.REQUEST_PROPERTY_KEY, new RequestRoutingProperties(request));
                return pipeline.Handle(request, cancellationToken);
            }

            var response = request.CreateResponse(System.Net.HttpStatusCode.BadGateway);
            response.Content = new StringContent("This request was not matched to any routing definition. Are you missing a routing definition?");

            return Task.FromResult(response);
        }

        private RequestPipleline GetPipeline(HttpRequestMessage request)
        {
            RequestPipleline mostMatchedPipeline = null;
            var score = 0;
            foreach (var pipeline in _serviceScripts.GetRequestPipelines())
            {
                var pipelineScore = pipeline.CanHandle(request);

                if (pipelineScore > score)
                {
                    mostMatchedPipeline = pipeline;
                    score = pipelineScore;
                }
            }

            return mostMatchedPipeline;
        }
    }
}
