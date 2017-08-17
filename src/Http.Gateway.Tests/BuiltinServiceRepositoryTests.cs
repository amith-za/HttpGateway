using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Http.Gateway.Registries;

namespace Http.Gateway.Tests
{
    [TestClass]
    public class BuiltinServiceRepositoryTests
    {
        [TestCategory("Integration")]
        [TestMethod]
        public void AddGetDeleteService()
        {
            var repo = new BuiltinServiceRepository();

            var service =
                new Service
                {
                    Name = Guid.NewGuid().ToString(),
                    Version = Guid.NewGuid().ToString()
                };

            repo.AddService(service);

            var read = repo.GetService(service.Name, service.Version);

            Assert.IsNotNull(read);

            repo.DeleteService(service);

            read = repo.GetService(service.Name, service.Version);

            Assert.IsNull(read);
        }

        [TestCategory("Integration")]
        [TestMethod]
        public void SaveGetUpdateDeleteServiceInstance()
        {
            var repo = new BuiltinServiceRepository();

            var service =
                new ServiceInstance
                {
                    Service = new Service
                    {
                        Name = Guid.NewGuid().ToString(),
                        Version = Guid.NewGuid().ToString()
                    },
                    id = Guid.NewGuid().ToString(),
                    Path = new Uri("http://localhost:1234/"),
                    Status = InstanceStatus.Unavailable
                };

            repo.SaveServiceInstance(service);

            var search = repo.GetServiceInstances(service.Service);

            Assert.IsNotNull(search);
            Assert.AreEqual(1, search.Count);

            var read = repo.GetServiceInstance(service.id);

            Assert.IsNotNull(read);

            service.Status = InstanceStatus.Active;
            repo.SaveServiceInstance(service);

            read = repo.GetServiceInstance(service.id);

            Assert.IsNotNull(read);
            Assert.AreEqual(InstanceStatus.Active, read.Status);

            repo.DeleteServiceInstance(service.id);

            read = repo.GetServiceInstance(service.id);

            Assert.IsNull(read);
        }
    }
}
