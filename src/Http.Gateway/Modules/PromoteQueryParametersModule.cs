using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Http.Gateway.Modules
{
    public class PromoteQueryParametersModule : Module
    {
        public List<string> Parameters { get; private set; }

        public override void Configure(ConfigurationMap configurationObject)
        {
            var parameters = configurationObject.GetValueOrDefault("params", new string[] { }) as string[];
            Parameters = new List<string>(parameters);
        }

        public override void Initialize(ConfigurationMap configurationObject)
        {

        }

        public override Task<HttpResponseMessage> Handle(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            foreach (var item in request.GetQueryNameValuePairs())
            {
                if (Parameters.Contains(item.Key, StringComparer.InvariantCultureIgnoreCase))
                {
                    request.AddUpdateRoutingProperty(item.Key, item.Value);
                }
            }

            return base.Handle(request, cancellationToken);
        }
    }
}
