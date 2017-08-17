using System.Collections.Generic;

namespace Http.Gateway.Registries
{
    public interface IServiceRepository
    {
        void AddService(Service service);
        void DeleteService(Service service);
        void DeleteServiceInstance(string id);
        string GetLatestVersionOfService(string serviceName);
        Service GetService(string serviceName, string serviceVersion);
        ServiceInstance GetServiceInstance(string instanceId);
        List<ServiceInstance> GetServiceInstances();
        List<ServiceInstance> GetServiceInstances(Service service);
        List<Service> GetServices();
        void SaveServiceInstance(ServiceInstance instance);
    }
}