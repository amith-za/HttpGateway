using Http.Gateway.Configuration;
using System.Collections.Generic;

namespace Http.Gateway
{
    public interface IModuleCatalog
    {
        Module CreateById(string moduleId);

        Module CreateById(string moduleId, ConfigurationMap configurationObject);

        List<Module> GetGlobalModules();

        List<string> GetAllowedModuleNames();
    }
}