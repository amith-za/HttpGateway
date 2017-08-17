using Http.Gateway.Configuration;
using RethinkDb.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway
{
    public class SyncedModuleCatalog : IModuleCatalog
    {
        private static SyncedScriptDataSet _modulesDataSet;

        static SyncedModuleCatalog()
        {
            _modulesDataSet = new SyncedScriptDataSet(Settings.RethinkDatabaseName, "basescripts", RethinkDB.R.Connection().Hostname(Settings.RethinkDatabaseHost).Port(Settings.RethinkDatabasePort));
            _modulesDataSet.StartSync();
        }

        public Module CreateById(string moduleId)
        {
            foreach (var item in GetModuleDefinitions())
            {
                if (string.Equals(item.Id, moduleId))
                {
                    try
                    {
                        var module = Activator.CreateInstance(item.ModuleType) as Module;
                        module.Initialize(item.ConfigurationObject);
                        return module;
                    }
                    catch
                    {
                        // Should log some message that there was a failure to initialize the Module
                        break;
                    }
                }
            }

            return null;
        }

        public Module CreateById(string moduleId, ConfigurationMap configurationObject)
        {
            var module = CreateById(moduleId);
            module?.Configure(configurationObject);

            return module;
        }

        public List<string> GetAllowedModuleNames()
        {
            return GetModuleDefinitions().Select(m => m.Id).ToList();
        }

        public List<Module> GetGlobalModules()
        {
            var globalmodules = System.Configuration.ConfigurationManager.AppSettings["httpgateway:globalmodules"] ?? "map_komodo_session;map_komodo_session_debug";

            var modules = new List<Module>();

            foreach (var item in globalmodules.Split(';'))
            {
                var module = CreateById(item.Trim(' '));

                if (module != null)
                {
                    modules.Add(module);
                }
            }

            return modules;
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
