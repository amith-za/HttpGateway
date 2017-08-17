using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway
{
    public class ServiceInstance
    {
        public Service Service { get; set; }

        public string id { get; set; }

        public Uri Path { get; set; }

        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public InstanceStatus Status { get; set; }
    }
}
