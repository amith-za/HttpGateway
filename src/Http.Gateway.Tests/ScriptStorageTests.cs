using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Http.Gateway.Tests
{
    [TestClass]
    public class ScriptStorageTests
    {
        [TestCategory("Integration")]
        [TestMethod]
        public void GetModulesScript()
        {
            using (var storage = new ScriptStorage())
            {
                var script = storage.GetModulesScript();
            }
        }

        [TestCategory("Integration")]
        [TestMethod]
        public void UpdateModulesScript()
        {
            using (var storage = new ScriptStorage())
            {
                storage.SaveModulesScript("module x is y");
            }
        }

        [TestCategory("Integration")]
        [TestMethod]
        public void GetRegistriesScript()
        {
            using (var storage = new ScriptStorage())
            {
                var script = storage.GetRegistriesScript();
            }
        }

        [TestCategory("Integration")]
        [TestMethod]
        public void UpdateRegistriesScript()
        {
            using (var storage = new ScriptStorage())
            {
                storage.SaveRegistriesScript("registry x is y");
            }
        }


        [TestCategory("Integration")]
        [TestMethod]
        public void SaveGetDeleteServiceScript()
        {
            var scriptName = "best";
            var script = "registry x is y";

            using (var storage = new ScriptStorage())
            {
                storage.SaveServiceScript(scriptName, script);

                var s = storage.GetServiceScriptsById(scriptName);

                Assert.IsNotNull(s);
                Assert.AreEqual(scriptName, s.id);
                Assert.AreEqual(script, s.content);

                storage.DeleteServiceScript(scriptName);

                Assert.IsNull(storage.GetServiceScriptsById(scriptName));
            }
        }
    }
}
