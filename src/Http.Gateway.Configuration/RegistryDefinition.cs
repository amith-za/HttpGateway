using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway.Configuration
{
    public class RegistryDefinition
    {
        public string Id { get; set; }

        public Type RegistryType { get; set; }

        public ConfigurationMap ConfigurationObject { get; set; }

        public RegistryDefinition(string id, string type, ConfigurationMap configuration)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentNullException("type");
            }

            RegistryType = Utility.ScanForType(type);

            if (RegistryType == null)
            {
                throw new ArgumentException($"Unable to resolve registry '{id}' using type '{type}'");
            }

            if (RegistryType.GetInterface("IRegistry") != typeof(IRegistry))
            {
                throw new ArgumentException($"Type '{type}' is not a valid registry. Does not implement '{typeof(IRegistry)}'");
            }

            Id = id;
            ConfigurationObject = configuration;
        }
    }
}
