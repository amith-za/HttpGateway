using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Http.Gateway.Configuration;
using Http.Gateway;

namespace Http.Gateway.Modules
{
    public class RequestDispatchModule : Module
    {
        private HttpClient client;

        public List<BackendDefinition> Backends { get; private set; }

        public List<RoutingRule> RoutingRules { get; private set; }

        public IRegistryCatalog RegistryCatalog { get; private set; }

        public RoutingRule DefaultRoute { get; private set; }

        public Dictionary<string, IRegistry> Registries { get; private set; }

        public RequestDispatchModule(RoutingDefinition routingDefinition, IRegistryCatalog registryCatalog)
        {
            Backends = routingDefinition.Backends;
            RoutingRules = routingDefinition.RoutingRules;
            RegistryCatalog = registryCatalog;

            if (routingDefinition.Validate().IsValid)
            {
                foreach (var routingRule in RoutingRules)
                {
                    if (routingRule.IsDefault)
                    {
                        DefaultRoute = routingRule;
                    }
                }
            }

            Registries = new Dictionary<string, IRegistry>();

            foreach (var backend in Backends)
            {
                try

                {
                    var registry = registryCatalog.GetRegistryById(backend.RegistryId, backend.RegistryConfiguration);
                    Registries.Add(backend.Id, registry);
                }
                catch (Exception e)
                {
                    throw new Exception($"Unable to create registry {backend.RegistryId} for backend {backend.Id}. See InnerException for details.", e);
                }
            }

            var httpClientHandler = new HttpClientHandler()
            {
                UseDefaultCredentials = true,
                UseProxy = false
            };

            client = new HttpClient(httpClientHandler);
        }

        public override void Configure(ConfigurationMap configurationObject)
        {
            throw new NotImplementedException();
        }

        public override void Initialize(ConfigurationMap configurationObject)
        {
            throw new NotImplementedException();
        }

        public override async Task<HttpResponseMessage> Handle(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestRoutingDetails = request.Properties[RequestRoutingProperties.REQUEST_PROPERTY_KEY] as RequestRoutingProperties;

            var routingRule = DetermineRoutingRule(requestRoutingDetails);

            var backend = Backends
                          .Where(b => string.Equals(b.Id, routingRule.BackendId, StringComparison.InvariantCultureIgnoreCase))
                          .FirstOrDefault();

            var backendUri = Registries[backend.Id].Next();

            if (backendUri == null)
            {
                var response =
                request.CreateResponse(System.Net.HttpStatusCode.ServiceUnavailable,
                    new
                    {
                        @class = new[] { "problem" },
                        title = "Service Unavailable",
                        problemType = "http://www.multichoice.co.za/problems/service-unavailable",
                        httpStatus = System.Net.HttpStatusCode.ServiceUnavailable,
                        detail = "Service is unavailable. Try again later.",
                        links = new[] { new { rel = new[] { "self" }, link = "http://www.multichoice.co.za/problems/service-unavailable" } }
                    });

                return await Task.FromResult(response);
            }

            request = await request.Clone();
            var uri = $"http://{backendUri.Host}:{backendUri.Port}/{backendUri.AbsolutePath}/{request.NormalizedPath()}";
            request.RequestUri = new Uri(uri);


            return await client.SendAsync(request);
        }

        private RoutingRule DetermineRoutingRule(RequestRoutingProperties requestRoutingDetails)
        {
            RoutingRule routingRule = null;

            foreach (var rule in RoutingRules)
            {
                if (rule.IsDefault)
                {
                    continue;
                }

                if (rule.Match(requestRoutingDetails))
                {
                    routingRule = rule;
                    break;
                }
            }

            if (routingRule == null)
            {
                routingRule = DefaultRoute;
            }

            return routingRule;
        }
    }
}
