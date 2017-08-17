using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway.Registries
{
    public class StaticRegistry : IRegistry
    {
        private int counter = 0;

        public List<Uri> Instances { get; set; }

        public StaticRegistry()
        {
            Instances = new List<Uri>();
        }

        public void Configure(ConfigurationMap configurationObject)
        {
            var instances = configurationObject.GetValueOrDefault("instances") as string[];
            if (instances != null)
            {
                var uris = instances.Select(x => new Uri(x));
                Instances.AddRange(uris);
            }
        }

        public void Initialize(ConfigurationMap configurationObject)
        {
        }

        public Uri Next()
        {
            counter++;

            var next = counter % Instances.Count;

            return Instances[next];
        }
    }
}
