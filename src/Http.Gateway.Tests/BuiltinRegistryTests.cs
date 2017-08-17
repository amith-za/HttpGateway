using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Http.Gateway.Registries;
using Moq;
using System.Collections.Generic;

namespace Http.Gateway.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class BuiltinRegistryTests
    {
        [TestMethod]
        public void ConstructedRegistryHasServiceRepository()
        {
            var registry = new BuiltinRegistry();

            Assert.IsNotNull(registry.ServiceRepository);
        }

        [TestMethod]
        public void ConfigureForSpecificVersion()
        {
            var registry = new BuiltinRegistry();

            var map = new ConfigurationMap();
            map.Add("service_name", "Hello");
            map.Add("use_version", "123");

            registry.Configure(map);

            Assert.AreEqual("Hello", registry.ServiceName);
            Assert.AreEqual("123", registry.UseVersion);
        }

        [TestMethod]
        public void UseSpecificVersionReturnsInstanceOfSpecificVersion()
        {
            var serviceInstances = new List<ServiceInstance>();
            serviceInstances.Add(
                new ServiceInstance
                {
                    Service = new Service
                    {
                        Name = "Hello",
                        Version = "1"
                    },
                    id = "Hello:1:localhost:8888",
                    Path = new Uri("http://localhost:8888/"),
                    Status = InstanceStatus.Active
                });

            var repo = new Mock<IServiceRepository>();
            repo
                .Setup(x => x.GetServiceInstances(It.Is<Service>(s => string.Equals(s.Name, "Hello") & string.Equals("1", s.Version))))
                .Returns(serviceInstances)
                .Verifiable();

            var registry = new BuiltinRegistry(repo.Object);

            var map = new ConfigurationMap();
            map.Add("service_name", "Hello");
            map.Add("use_version", "1");

            registry.Configure(map);

            var next = registry.Next();

            repo.Verify();

            Assert.IsNotNull(next);
            Assert.AreEqual("http://localhost:8888/", next.ToString());
        }

        [TestMethod]
        public void UseSpecificVersionRoundRobinsHosts()
        {
            var serviceInstances = new List<ServiceInstance>();
            serviceInstances.Add(
                new ServiceInstance
                {
                    Service = new Service
                    {
                        Name = "Hello",
                        Version = "1"
                    },
                    id = "Hello:1:localhost:8888",
                    Path = new Uri("http://localhost:8888/"),
                    Status = InstanceStatus.Active
                });

            serviceInstances.Add(
                new ServiceInstance
                {
                    Service = new Service
                    {
                        Name = "Hello",
                        Version = "123"
                    },
                    id = "Hello:123:localhost:7777",
                    Path = new Uri("http://localhost:7777/"),
                    Status = InstanceStatus.Active
                });

            var repo = new Mock<IServiceRepository>();
            repo
                .Setup(x => x.GetServiceInstances(It.Is<Service>(s => string.Equals(s.Name, "Hello") & string.Equals("1", s.Version))))
                .Returns(serviceInstances);

            var registry = new BuiltinRegistry(repo.Object);

            var map = new ConfigurationMap();
            map.Add("service_name", "Hello");
            map.Add("use_version", "1");

            registry.Configure(map);

            var next = registry.Next();
            Assert.IsNotNull(next);
            Assert.AreEqual("http://localhost:7777/", next.ToString());

            next = registry.Next();
            Assert.IsNotNull(next);
            Assert.AreEqual("http://localhost:8888/", next.ToString());

            next = registry.Next();
            Assert.IsNotNull(next);
            Assert.AreEqual("http://localhost:7777/", next.ToString());

            next = registry.Next();
            Assert.IsNotNull(next);
            Assert.AreEqual("http://localhost:8888/", next.ToString());
        }

        [TestMethod]
        public void ConfigureForLatestVersion()
        {
            var registry = new BuiltinRegistry();

            var map = new ConfigurationMap();
            map.Add("service_name", "Hello");
            map.Add("use_latest", "true");

            registry.Configure(map);

            Assert.AreEqual("Hello", registry.ServiceName);
            Assert.AreEqual(null, registry.UseVersion);
            Assert.AreEqual(true, registry.UseLatest);
            Assert.AreEqual(false, registry.UseStable);
        }

        [TestMethod]
        public void UseLatestVersionReturnsInstanceOfLatestVersion()
        {
            var serviceInstances = new List<ServiceInstance>();
            serviceInstances.Add(
                new ServiceInstance
                {
                    Service = new Service
                    {
                        Name = "Hello",
                        Version = "2"
                    },
                    id = "Hello:2:localhost:6666",
                    Path = new Uri("http://localhost:6666/"),
                    Status = InstanceStatus.Active
                });

            var repo = new Mock<IServiceRepository>();
            repo
                .Setup(x => x.GetServiceInstances(It.Is<Service>(s => string.Equals(s.Name, "Hello") & string.Equals("2", s.Version))))
                .Returns(serviceInstances)
                .Verifiable();
            repo.Setup(x => x.GetLatestVersionOfService("Hello"))
                .Returns("2");

            var registry = new BuiltinRegistry(repo.Object);

            var map = new ConfigurationMap();
            map.Add("service_name", "Hello");
            map.Add("use_latest", "true");

            registry.Configure(map);

            var next = registry.Next();

            repo.Verify();

            Assert.IsNotNull(next);
            Assert.AreEqual("http://localhost:6666/", next.ToString());
        }
    }
}
