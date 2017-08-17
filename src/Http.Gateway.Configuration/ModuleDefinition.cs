using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway.Configuration
{
    public class ModuleDefinition
    {
        public string Id { get; set; }

        public Type ModuleType { get; set; }

        public ConfigurationMap ConfigurationObject { get; private set; }

        public string ConfigurationErrorMessage { get; private set; }

        public ModuleDefinition(string id, string type, ConfigurationMap configuration)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentNullException("type");
            }

            ModuleType = Utility.ScanForType(type);

            if (ModuleType == null)
            {
                throw new ArgumentException($"Unable to resolve type '{type}' for module '{id}'");
            }

            if (!typeof(Module).IsAssignableFrom(ModuleType))
            {
                throw new ArgumentException($"Type '{type}' is not a valid Module. Does not inherit '{typeof(Module)}'");
            }

            Id = id;
            ConfigurationObject = configuration;
        }

        public bool IsValid()
        {
            try
            {
                var module = Activator.CreateInstance(ModuleType) as Module;
                module.Initialize(ConfigurationObject);
            }
            catch (Exception e)
            {
                ConfigurationErrorMessage = e.Message;
                return false;
            }

            return true;
        }
    }
}
