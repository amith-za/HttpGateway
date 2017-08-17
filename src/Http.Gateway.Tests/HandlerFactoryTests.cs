using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Http.Gateway.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class HandlerFactoryTests
    {
        //[TestMethod]
        //public void Creating_A_Handler_Creates_A_Handler()
        //{
        //    var handler = new ModuleCatalog().CreateModule(typeof(TestHandler), null);

        //    Assert.IsNotNull(handler);
        //    Assert.IsInstanceOfType(handler, typeof(TestHandler));
        //}

        //[ExpectedException(typeof(ArgumentException))]
        //[TestMethod]
        //public void Creating_A_Handler_That_Is_A_Class_But_Does_Not_Implement_IHandler_Throws_An_ArgumentException()
        //{
        //    var handler = new ModuleCatalog().CreateModule(typeof(ModuleCatalog), null);
        //}

        //[TestMethod]
        //public void When_Creating_A_Handler_With_A_Configuration_Object_The_Configuration_Properties_Are_Set()
        //{
        //    var configurationObject = new ConfigurationMap()
        //    {
        //        { "AStringSetting", "X" },
        //        { "AnIntSetting", "10" }
        //    };

        //    var handler = new ModuleCatalog().CreateModule(typeof(TestHandler), configurationObject) as TestHandler;

        //    Assert.AreEqual(configurationObject["AStringSetting"], handler.AStringSetting);
        //    Assert.AreEqual(10, handler.AnIntSetting);
        //}
    }
}
