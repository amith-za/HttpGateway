using Http.Gateway.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Http.Gateway.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class RouteInfoTests
    {
        [TestMethod]
        public void ConstructingRouteInfoSucceeds()
        {
            var routeInfo = new RouteInfo("GET", "{country}/accounts/{accountId}");
        }

        [TestMethod]
        public void ConstructingRouteInfoHasCorrectProperties()
        {
            var routeInfo = new RouteInfo("GET", "{country}/accounts/{accountId}");

            Assert.AreEqual("GET", routeInfo.HttpMethod);
            Assert.AreEqual("{country}/accounts/{accountId}", routeInfo.RouteTemplate);
        }


        [TestMethod]
        public void ValidRouteIsMatched()
        {
            var routeInfo = new RouteInfo("GET", "{country}/accounts/{accountId}");

            Assert.AreEqual(8, routeInfo.Match("GET", "zaf/accounts/12345"));
        }

        [TestMethod]
        public void ValidRouteIsNotMatchedWhenVerbIsIncorrect()
        {
            var routeInfo = new RouteInfo("GET", "{country}/accounts/{accountId}");

            Assert.AreEqual(0, routeInfo.Match("POST", "zaf/accounts/12345"));
        }

        [TestMethod]
        public void InValidRouteIsNotMatched()
        {
            var routeInfo = new RouteInfo("GET", "{country}/accounts/{accountId}");

            Assert.AreEqual(0, routeInfo.Match("GET", "zaf/account/12345"));
        }

        [TestMethod]
        public void OptionsRequestForRouteIsMatched()
        {
            var routeInfo = new RouteInfo("GET", "{country}/accounts/{accountId}");

            Assert.AreEqual(8, routeInfo.Match("OPTIONS", "zaf/accounts/12345"));
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullHttpVerbResultsInArgumentNullException()
        {
            var routeInfo = new RouteInfo(null, "{country}/accounts/{accountId}");
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void EmptyStringHttpVerbResultsInArgumentNullException()
        {
            var routeInfo = new RouteInfo("", "{country}/accounts/{accountId}");
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void WhiteSpaceHttpVerbResultsInArgumentNullException()
        {
            var routeInfo = new RouteInfo(" ", "{country}/accounts/{accountId}");
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullRouteTemplateResultsInArgumentNullException()
        {
            var routeInfo = new RouteInfo("GET", null);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void EmptyStringRouteTemplateResultsInArgumentNullException()
        {
            var routeInfo = new RouteInfo("GET", "");
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void WhiteSpaceRouteTemplateResultsInArgumentNullException()
        {
            var routeInfo = new RouteInfo("GET", "    ");
        }
    }
}
