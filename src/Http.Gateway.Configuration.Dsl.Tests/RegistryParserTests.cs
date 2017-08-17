using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Http.Gateway.Configuration.Dsl.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class RegistryParserTests
    {
        [TestMethod]
        public void ConstructParser()
        {
            var parser = new RegistryParser();
        }

        [TestMethod]
        public void ParseRegistryDefinition()
        {
            var registryName = "bultin_registry";
            var registryType = typeof(TestRegistry);

            var parser = new RegistryParser();

            var result = parser.ParseRegistries($"registry {registryName} is {registryType.FullName} {{ setting1 = \"blah\" }}");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Result.Count);
            Assert.AreEqual(registryName, result.Result[0].Id);
            Assert.AreEqual(registryType, result.Result[0].RegistryType);
        }

        [TestMethod]
        public void ParseRegistryDefinitions()
        {
            var registryIdentifier1 = "bultin_registry";
            var registryIdentifier2 = "consul_registry";
            var registryType = typeof(TestRegistry);

            var parser = new RegistryParser();

            var moduleDefinition =
$@"
registry {registryIdentifier1} is {registryType.FullName} {{ setting1 = ""blah"" }} 
registry {registryIdentifier2} is {registryType.FullName} {{ setting1 = ""blah blah blah"" }}";

            var result = parser.ParseRegistries(moduleDefinition);

            Console.WriteLine(moduleDefinition);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Result.Count);
            Assert.AreEqual(registryIdentifier1, result.Result[0].Id);
            Assert.AreEqual(registryIdentifier2, result.Result[1].Id);
            Assert.AreEqual(registryType, result.Result[0].RegistryType);
            Assert.AreEqual(registryType, result.Result[1].RegistryType);
        }

        [TestMethod]
        public void OmittingModuleResultsInParserErrorMessage()
        {
            var registryName = "bultin_registry";
            var registryType = typeof(TestModule);

            var parser = new RegistryParser();

            var result = parser.ParseRegistries($"module {registryName} is {registryType.FullName} {{ setting1 = \"blah\" }}");

            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.ParserMessages.Count);
        }

        [TestMethod]
        public void OmittingIsResultsInParserErrorMessage()
        {
            var moduleName = "bultin_registry";
            var moduleType = typeof(TestModule);

            var parser = new RegistryParser();

            var result = parser.ParseRegistries($"registry {moduleName} as {moduleType.FullName} {{ setting1 = \"blah\" }}");

            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.ParserMessages.Count);
        }
    }
}
