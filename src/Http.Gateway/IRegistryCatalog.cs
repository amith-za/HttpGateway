using Http.Gateway.Configuration;
using System.Collections.Generic;

namespace Http.Gateway
{
    public interface IRegistryCatalog
    {
        List<RegistryDefinition> GetRegistryDefinitions();

        IRegistry GetRegistryById(string registryId);

        IRegistry GetRegistryById(string registryId, ConfigurationMap configurationObject);
    }
}