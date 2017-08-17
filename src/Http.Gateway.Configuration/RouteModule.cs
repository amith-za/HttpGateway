using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway.Configuration
{
    public class RouteModule
    {
        public string Id { get; set; }

        public ConfigurationMap Configuration { get; set; }
    }
}
