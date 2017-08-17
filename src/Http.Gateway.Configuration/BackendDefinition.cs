using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway.Configuration
{
    public class BackendDefinition
    {
        public string Id { get; set; }

        public string RegistryId { get; set; }

        public ConfigurationMap RegistryConfiguration { get; set; }
    }
}
