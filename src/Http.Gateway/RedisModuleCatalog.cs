using Http.Gateway.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway
{
    public class RedisModuleCatalog : IModuleCatalog
    {
        private ConnectionMultiplexer _connection;
        private List<ModuleDefinition> _modules;

        internal RedisModuleCatalog()
        {
            _connection = ConnectionMultiplexer.Connect("localhost");
#warning create a enum for the database numbers
            var db = _connection.GetDatabase(0);
            var modulesScript = db.StringGet("global-configuration-modules");

            _modules = new Configuration.Dsl.ModuleParser().ParseModules(modulesScript).Result;

            var subscriber = _connection.GetSubscriber();

            subscriber.Subscribe("__keyspace@0__:global-configuration-modules", (channel, value) =>
             {
                 
             });
        }

        public Module CreateById(string moduleId)
        {
            throw new NotImplementedException();
        }

        public Module CreateById(string moduleId, ConfigurationMap configurationObject)
        {
            throw new NotImplementedException();
        }

        public List<string> GetAllowedModuleNames()
        {
            throw new NotImplementedException();
        }

        public List<Module> GetGlobalModules()
        {
            throw new NotImplementedException();
        }

        private List<ModuleDefinition> GetModuleDefinitions()
        {
            var moduleScript = _modulesDataSet.GetScript("_modules");

            if (moduleScript == null)
            {
                return new List<ModuleDefinition>();
                //throw new InvalidOperationException("Could not load modules");
            }

            var modules = new Configuration.Dsl.ModuleParser().ParseModules(moduleScript.content).Result;

            return modules;
        }
    }
}
