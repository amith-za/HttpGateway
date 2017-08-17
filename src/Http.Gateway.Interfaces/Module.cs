using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Http.Gateway
{
    public abstract class Module
    {
        public string Id { get; set; }

        public Module InnerHandler { get; set; }

        public Module()
        { }

        public Module(Module innerHandler)
        {
            InnerHandler = innerHandler;
        }

        public abstract void Initialize(ConfigurationMap configurationObject);

        public abstract void Configure(ConfigurationMap configurationObject);

        public virtual Task<HttpResponseMessage> Handle(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return InnerHandler.Handle(request, cancellationToken);
        }
    }
}
