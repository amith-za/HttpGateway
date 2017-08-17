using Http.Gateway.Configuration;
using RethinkDb.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway
{
    public class SyncedRegistryCatalog : IRegistryCatalog
    {
        private static SyncedScriptDataSet _registriesDataSet;

        static SyncedRegistryCatalog()
        {
            _registriesDataSet = new SyncedScriptDataSet(Settings.RethinkDatabaseName, "basescripts", RethinkDB.R.Connection().Hostname(Settings.RethinkDatabaseHost).Port(Settings.RethinkDatabasePort));
            _registriesDataSet.StartSync();
        }

        public SyncedRegistryCatalog()
        {
        }

        public List<RegistryDefinition> GetRegistryDefinitions()
        {
            var registyScript = _registriesDataSet.GetScript("_registries");

            if (registyScript == null)
            {
                return new List<RegistryDefinition>();
                //throw new InvalidOperationException("Could not load registries");
            }

            var registries = new Configuration.Dsl.RegistryParser().ParseRegistries(registyScript.content).Result;

            return registries;
        }

        public IRegistry GetRegistryById(string registryId)
        {
            foreach (var item in GetRegistryDefinitions())
            {
                if (string.Equals(item.Id, registryId))
                {
                    var registry = Activator.CreateInstance(item.RegistryType) as IRegistry;
                    registry.Initialize(item.ConfigurationObject);
                    return registry;
                }
            }

            return null;
        }

        public IRegistry GetRegistryById(string registryId, ConfigurationMap configurationObject)
        {
            var module = GetRegistryById(registryId);
            module?.Configure(configurationObject);

            return module;
        }
    }
}
