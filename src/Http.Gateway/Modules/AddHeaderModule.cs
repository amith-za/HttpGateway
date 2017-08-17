using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Http.Gateway.Modules
{
    public class AddHeaderModule : Module
    {
        public string Header { get; private set; }

        public string Value { get; private set; }

        public override void Configure(ConfigurationMap configurationObject)
        {
            Header = configurationObject.GetValueOrDefault("header") as string;
            Value = configurationObject.GetValueOrDefault("value") as string;
        }

        public override void Initialize(ConfigurationMap configurationObject)
        {
            
        }

        public override Task<HttpResponseMessage> Handle(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(Header) &
                !string.IsNullOrWhiteSpace(Value))
            {
                request.Headers.Add(Header, Value);
            }

            return base.Handle(request, cancellationToken);
        }
    }
}
