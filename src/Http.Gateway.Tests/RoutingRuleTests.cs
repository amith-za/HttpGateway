using Http.Gateway.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;

namespace Http.Gateway.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class RoutingRuleTests
    {
        [TestMethod]
        public void ConstructedRuleIsCorrect()
        {
            var rule = new RoutingRule("1==1", "account.static");

            Assert.AreEqual("account.static", rule.BackendId);
            Assert.AreEqual("1==1", rule.Expression);
        }

        [TestMethod]
        public void ValidScriptIsValid()
        {
            var rule = new RoutingRule("1==1", "account.static");

            Assert.IsTrue(rule.IsValid);
        }

        [TestMethod]
        public void InValidScriptIsInValid()
        {
            var rule = new RoutingRule("blah", "account.static");

            Assert.IsFalse(rule.IsValid);
        }

        [TestMethod]
        public void InValidScriptHasErrorMessage()
        {
            var rule = new RoutingRule("blah", "account.static");

            Assert.IsFalse(string.IsNullOrWhiteSpace(rule.ExpressionError));
        }

        [TestMethod]
        public void WhenInvalidScriptMatchIsFalse()
        {
            var rule = new RoutingRule("blah", "account.static");
            Assert.IsFalse(rule.IsValid);

            var request = new HttpRequestMessage();
            request.Properties.Add("user-data", new UserData("zaf", "frikkie"));

            var shouldRoute = rule.Match(new RequestRoutingProperties(request));
            Assert.IsFalse(shouldRoute);
        }

        [TestMethod]
        public void MatchIsTrueWhenExpressionIsMatched()
        {
            var rule = new RoutingRule("request[\"user\"] == \"frikkie\"", "x");

            Assert.IsTrue(rule.IsValid);

            var request = new HttpRequestMessage();
            request.Properties.Add("user-data", new UserData("zaf", "frikkie"));

            var shouldRoute = rule.Match(new RequestRoutingProperties(request));

            Assert.IsTrue(shouldRoute);
        }

        [TestMethod]
        public void MatchIsFalseWhenExpressionIsNotMatched()
        {
            var rule = new RoutingRule("request[\"user\"] == \"frikkie\"", "x");
            Assert.IsTrue(rule.IsValid);

            var request = new HttpRequestMessage();
            request.Properties.Add("user-data", new UserData("zaf", "bob"));

            var shouldRoute = rule.Match(new RequestRoutingProperties(request));

            Assert.IsFalse(shouldRoute);
        }

        [TestMethod]
        public void EmptyExpressionIndicatesValidAndDefault()
        {
            var rule = new RoutingRule("", "x");
            Assert.IsTrue(rule.IsValid);

            Assert.IsTrue(rule.IsValid);
            Assert.IsTrue(rule.IsDefault);
        }

        [TestMethod]
        public void WhenDefaultMatchIsTrue()
        {
            var rule = new RoutingRule("", "x");
            Assert.IsTrue(rule.IsValid);

            var request = new HttpRequestMessage();
            request.Properties.Add("user-data", new UserData("zaf", "bob"));

            var shouldRoute = rule.Match(new RequestRoutingProperties(request));

            Assert.IsTrue(shouldRoute);
        }

        [TestMethod]
        public void EmptyExpressionWithBracesIndicatesInvalid()
        {
            var rule = new RoutingRule("{}", "x");

            Assert.IsFalse(rule.IsValid);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullBackEndThrowsArgumentNullException()
        {
            var rule = new RoutingRule("request.user == \"frikkie\"", null);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void EmptyStringBackEndThrowsArgumentNullException()
        {
            var rule = new RoutingRule("request.user == \"frikkie\"", "");
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void WhitespaceBackEndThrowsArgumentNullException()
        {
            var rule = new RoutingRule("request.user == \"frikkie\"", "     ");
        }


    }
}
