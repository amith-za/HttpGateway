using Http.Gateway.Configuration.Dsl;
using Http.Gateway.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;

namespace Http.Gateway.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class PipelineFactoryTests
    {
        PipelineFactory Create()
        {
            //var modules = new Configuration.Dsl.ModuleParser().ParseModules(Utility.ReadScript("modules.txt")).Result;
            var moduleCatalog = new Mock<IModuleCatalog>();
            moduleCatalog.Setup(m => m.GetGlobalModules()).Returns(new System.Collections.Generic.List<Module>() { });

            var registry = new Mock<IRegistry>();

            var registryCatalog = new Mock<IRegistryCatalog>();
            registryCatalog.Setup(m => m.GetRegistryById("")).Returns(registry.Object);

            //var registries = new Configuration.Dsl.RegistryParser().ParseRegistries(Utility.ReadScript("registries.txt")).Result;
            //var registryCatalog = new SyncedRegistryCatalog(registries);

            var pipelineFactory = new PipelineFactory(moduleCatalog.Object, registryCatalog.Object);

            return pipelineFactory;
        }

        [TestMethod]
        public void CreatedPipelineHasTheRightNumberAndTypeOfModules()
        {
            var pipelineFactory = Create();

            var parser = new RoutingDefinitionParser(new List<string> { "static_list", "builtin" }, new List<string>() { "none" });
            var routingDefinition = parser.ParseRoutingDefinition(Utility.ReadScript("routingscript1.txt"));

            var pipeline = pipelineFactory.CreatePipeline(routingDefinition.Result);

            Assert.IsNotNull(pipeline.PipelineModules);

            Assert.AreEqual(1, pipeline.PipelineModules.Count);
            
            Assert.IsInstanceOfType(pipeline.PipelineModules[0], typeof(RequestDispatchModule));
        }

        [TestMethod]
        public void CreatedPipelineHasCorrectRoutes()
        {
            var pipelineFactory = Create();

            var parser = new RoutingDefinitionParser(new List<string> { "static_list", "builtin" }, new List<string>() { "none" });
            var routingDefinition = parser.ParseRoutingDefinition(Utility.ReadScript("routingscript1.txt"));

            var pipeline = pipelineFactory.CreatePipeline(routingDefinition.Result);

            Assert.IsNotNull(pipeline.HandledRoutes);

            Assert.AreEqual(3, pipeline.HandledRoutes.Count);

            Assert.AreEqual("GET", pipeline.HandledRoutes[0].HttpMethod);
            Assert.AreEqual("{country}/accounts", pipeline.HandledRoutes[0].RouteTemplate);

            Assert.AreEqual("PUT", pipeline.HandledRoutes[1].HttpMethod);
            Assert.AreEqual("{country}/accounts/{accountId}", pipeline.HandledRoutes[1].RouteTemplate);

            Assert.AreEqual("POST", pipeline.HandledRoutes[2].HttpMethod);
            Assert.AreEqual("{country}/accounts", pipeline.HandledRoutes[2].RouteTemplate);
        }

        [TestMethod]
        public void CreatedPipelineCanHandleRquestThatItShould()
        {
            var pipelineFactory = Create();

            var parser = new RoutingDefinitionParser(new List<string> { "static_list", "builtin" }, new List<string>() { "none" });
            var routingDefinition = parser.ParseRoutingDefinition(Utility.ReadScript("routingscript1.txt"));

            var pipeline = pipelineFactory.CreatePipeline(routingDefinition.Result);

            var request = new System.Net.Http.HttpRequestMessage(HttpMethod.Get, "http://localhost/zaf/accounts");
            var httpConfig = new HttpConfiguration();
            request.Properties[System.Web.Http.Hosting.HttpPropertyKeys.HttpConfigurationKey] = httpConfig;

            Assert.IsTrue(pipeline.CanHandle(request));
        }

        [TestMethod]
        public void CreatedPipelineWontHandleRquestThatItShouldnt()
        {
            var pipelineFactory = Create();

            var parser = new RoutingDefinitionParser(new List<string> { "static_list", "builtin" }, new List<string>() { "none" });
            var routingDefinition = parser.ParseRoutingDefinition(Utility.ReadScript("routingscript1.txt"));

            var pipeline = pipelineFactory.CreatePipeline(routingDefinition.Result);

            var request = new System.Net.Http.HttpRequestMessage(HttpMethod.Get, "http://localhost/zaf/customers");
            var httpConfig = new HttpConfiguration();
            request.Properties[System.Web.Http.Hosting.HttpPropertyKeys.HttpConfigurationKey] = httpConfig;

            Assert.IsFalse(pipeline.CanHandle(request));
        }
    }
}
