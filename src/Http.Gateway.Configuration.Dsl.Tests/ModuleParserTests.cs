using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Http.Gateway.Configuration.Dsl.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class ModuleParserTests
    {
        [TestMethod]
        public void ConstructParser()
        {
            var parser = new ModuleParser();
        }

        [TestMethod]
        public void ParseModuleDefinition()
        {
            var moduleName = "map_komodo_session";
            var moduleType = typeof(TestModule);

            var parser = new ModuleParser();

            var result = parser.ParseModules($"module {moduleName} is {moduleType.FullName} {{ setting1 = \"blah\" }}");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Result.Count);
            Assert.AreEqual(moduleName, result.Result[0].Id);
            Assert.AreEqual(moduleType, result.Result[0].ModuleType);
        }

        [TestMethod]
        public void ParseModuleDefinitions()
        {
            var moduleIdentifier1 = "map_komodo_session";
            var moduleIdentifier2 = "map_komodo_session2";
            var moduleType = typeof(TestModule);

            var parser = new ModuleParser();

            var moduleDefinition = 
$@"
module {moduleIdentifier1} is {moduleType.FullName} {{ setting1 = ""blah"", number = 12 }} 
module {moduleIdentifier2} is {moduleType.FullName} {{ setting1 = ""blah blah blah"" }}";

            var result = parser.ParseModules(moduleDefinition);

            Console.WriteLine(moduleDefinition);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Result.Count);
            Assert.AreEqual(moduleIdentifier1, result.Result[0].Id);
            Assert.AreEqual(moduleIdentifier2, result.Result[1].Id);
            Assert.AreEqual(moduleType, result.Result[0].ModuleType);
            Assert.AreEqual(moduleType, result.Result[1].ModuleType);
        }

        [TestMethod]
        public void OmittingModuleResultsInParserErrorMessage()
        {
            var moduleName = "map_komodo_session";
            var moduleType = typeof(TestModule);

            var parser = new ModuleParser();

            var result = parser.ParseModules($"odule {moduleName} is {moduleType.FullName} {{ setting1 = \"blah\" }}");

            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.ParserMessages.Count);
        }

        [TestMethod]
        public void OmittingIsResultsInParserErrorMessage()
        {
            var moduleName = "map_komodo_session";
            var moduleType = typeof(TestModule);

            var parser = new ModuleParser();

            var result = parser.ParseModules($"module {moduleName} as {moduleType.FullName} {{ setting1 = \"blah\" }}");

            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.ParserMessages.Count);
        }
    }
}
