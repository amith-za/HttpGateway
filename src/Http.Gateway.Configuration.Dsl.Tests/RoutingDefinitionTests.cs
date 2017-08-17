using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Http.Gateway.Configuration.Dsl.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class RoutingDefinitionTests
    {
        [TestMethod]
        public void ParsingValidScriptIsWithoutErrors()
        {
            var script = Utility.ReadScript("routingscript1.txt");

            var parser = new RoutingDefinitionParser(new List<string> { "static_list", "builtin" }, new List<string>() { "none"});

            var parseResult = parser.ParseRoutingDefinition(script);

            Assert.IsNotNull(parseResult);
            Assert.IsFalse(parseResult.HasErrors);
            Assert.IsNotNull(parseResult.Result);
        }

        [TestMethod]
        public void ParsingValidScriptHasCorrectBackends()
        {
            var script = Utility.ReadScript("routingscript1.txt");

            var parser = new RoutingDefinitionParser(new List<string> { "static_list", "builtin" }, new List<string>() { "none" });

            var parseResult = parser.ParseRoutingDefinition(script);

            Assert.IsFalse(parseResult.HasErrors);

            Assert.IsNotNull(parseResult.Result.Backends);
            Assert.AreEqual(3, parseResult.Result.Backends.Count);

            Assert.IsNotNull(parseResult.Result.Backends[0]);
            Assert.AreEqual("account.stable", parseResult.Result.Backends[0].Id);
            Assert.AreEqual("builtin", parseResult.Result.Backends[0].RegistryId);

            Assert.IsNotNull(parseResult.Result.Backends[1]);
            Assert.AreEqual("account.experimental", parseResult.Result.Backends[1].Id);
            Assert.AreEqual("builtin", parseResult.Result.Backends[1].RegistryId);

            Assert.IsNotNull(parseResult.Result.Backends[2]);
            Assert.AreEqual("account.static", parseResult.Result.Backends[2].Id);
            Assert.AreEqual("static_list", parseResult.Result.Backends[2].RegistryId);
        }

        [TestMethod]
        public void ParsingValidScriptHasCorrectBackendConfig()
        {
            var script = Utility.ReadScript("routingscript1.txt");

            var parser = new RoutingDefinitionParser(new List<string> { "static_list", "builtin" }, new List<string>() { "map_komodo_session" });

            var parseResult = parser.ParseRoutingDefinition(script);

            Assert.IsFalse(parseResult.HasErrors);

            Assert.IsTrue(parseResult.Result.Backends[0].RegistryConfiguration.ContainsKey("service"));
            Assert.AreEqual("api.account", (string)parseResult.Result.Backends[0].RegistryConfiguration["service"]);

            Assert.IsTrue(parseResult.Result.Backends[0].RegistryConfiguration.ContainsKey("use_stable"));
            Assert.AreEqual("true", (string)parseResult.Result.Backends[0].RegistryConfiguration["use_stable"]);
        }

        [TestMethod]
        public void ParsingValidScriptHasCorrectRoutes()
        {
            var script = Utility.ReadScript("routingscript1.txt");

            var parser = new RoutingDefinitionParser(new List<string> { "static_list", "builtin" }, new List<string>() { "none" });

            var parseResult = parser.ParseRoutingDefinition(script);

            Assert.IsFalse(parseResult.HasErrors);

            Assert.IsNotNull(parseResult.Result.Routes);
            Assert.AreEqual(3, parseResult.Result.Routes.Count);

            Assert.AreEqual("GET", parseResult.Result.Routes[0].HttpMethod);
            Assert.AreEqual("{country}/accounts", parseResult.Result.Routes[0].RouteTemplate);

            Assert.AreEqual("PUT", parseResult.Result.Routes[1].HttpMethod);
            Assert.AreEqual("{country}/accounts/{accountId}", parseResult.Result.Routes[1].RouteTemplate);

            Assert.AreEqual("POST", parseResult.Result.Routes[2].HttpMethod);
            Assert.AreEqual("{country}/accounts", parseResult.Result.Routes[2].RouteTemplate);
        }

        [TestMethod]
        public void ParsingValidScriptHasCorrectRoutingRules()
        {
            var script = Utility.ReadScript("routingscript1.txt");

            var parser = new RoutingDefinitionParser(new List<string> { "static_list", "builtin" }, new List<string>() { "none" });

            var parseResult = parser.ParseRoutingDefinition(script);

            Assert.IsFalse(parseResult.HasErrors);

            Assert.IsNotNull(parseResult.Result.RoutingRules);
            Assert.AreEqual(2, parseResult.Result.RoutingRules.Count);

            Assert.IsTrue(parseResult.Result.RoutingRules[0].Expression.Replace(" ", "") == string.Empty);
            Assert.AreEqual("account.stable", parseResult.Result.RoutingRules[0].BackendId);
            Assert.IsTrue(parseResult.Result.RoutingRules[0].IsDefault);

            Assert.AreEqual("request[\"user\"] == \"za\\\\amith.sewnarain\"", parseResult.Result.RoutingRules[1].Expression);
            Assert.AreEqual("account.experimental", parseResult.Result.RoutingRules[1].BackendId);
            Assert.IsFalse(parseResult.Result.RoutingRules[1].IsDefault);
        }
    }
}
