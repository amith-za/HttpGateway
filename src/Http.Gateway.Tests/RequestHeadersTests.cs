using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Http.Gateway.Configuration;

namespace Http.Gateway.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class RequestHeadersTests
    {
        [TestMethod]
        public void ConstructSuccessfully()
        {
            var request = new System.Net.Http.HttpRequestMessage();

            var requestHeaders = new RequestHeaders(request.Headers);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void ConstructorThrowsNullArgumentExceptioWhenHeadersAreNull()
        {
            var requestHeaders = new RequestHeaders(null);
        }

        [TestMethod]
        public void SaysItContainsHeaderWhenItIsPresent()
        {
            var request = new System.Net.Http.HttpRequestMessage();
            request.Headers.Add("x-random", "42");

            var requestHeaders = new RequestHeaders(request.Headers);

            Assert.IsTrue(requestHeaders.hasHeader("x-random"));
        }

        [TestMethod]
        public void SaysItDoesntContainHeaderWhenItIsntPresent()
        {
            var request = new System.Net.Http.HttpRequestMessage();

            var requestHeaders = new RequestHeaders(request.Headers);

            Assert.IsFalse(requestHeaders.hasHeader("x-random"));
        }

        [TestMethod]
        public void HasHeaderValueWhenPresent()
        {
            var request = new System.Net.Http.HttpRequestMessage();
            request.Headers.Add("x-random", "42");

            var requestHeaders = new RequestHeaders(request.Headers);

            Assert.IsTrue(requestHeaders["x-random"].hasValue("42"));
        }

        [TestMethod]
        public void HasHeaderValuesWhenPresent()
        {
            var request = new System.Net.Http.HttpRequestMessage();
            request.Headers.Add("x-random", "12");
            request.Headers.Add("x-random", "99");

            var requestHeaders = new RequestHeaders(request.Headers);

            Assert.IsTrue(requestHeaders["x-random"].hasValue("12"));
            Assert.IsTrue(requestHeaders["x-random"].hasValue("99"));
        }

        [TestMethod]
        public void DoesntHaveHeaderValueWhenNotPresent()
        {
            var request = new System.Net.Http.HttpRequestMessage();
            request.Headers.Add("x-random", "42");

            var requestHeaders = new RequestHeaders(request.Headers);

            Assert.IsFalse(requestHeaders["x-random"].hasValue("19"));
        }

        [TestMethod]
        public void DoesntHaveHeaderValueWhenHeaderIsntPresent()
        {
            var request = new System.Net.Http.HttpRequestMessage();
            var requestHeaders = new RequestHeaders(request.Headers);

            Assert.IsFalse(requestHeaders["x-random"].hasValue("19"));
        }

        [TestMethod]
        public void HasHeaderIsCaseInsensitiveByDefault()
        {
            var request = new System.Net.Http.HttpRequestMessage();
            request.Headers.Add("x-random", "aBc");

            var requestHeaders = new RequestHeaders(request.Headers);

            Assert.IsTrue(requestHeaders["x-random"].hasValue("abc"));
        }

        [TestMethod]
        public void HasHeaderIsCaseInsensitiveWhenExplicit()
        {
            var request = new System.Net.Http.HttpRequestMessage();
            request.Headers.Add("x-random", "aBc");

            var requestHeaders = new RequestHeaders(request.Headers);

            Assert.IsTrue(requestHeaders["x-random"].hasValue("aBc", true));
        }

        [TestMethod]
        public void HasHeaderIsCaseSensitiveWhenRequired()
        {
            var request = new System.Net.Http.HttpRequestMessage();
            request.Headers.Add("x-random", "aBc");

            var requestHeaders = new RequestHeaders(request.Headers);

            Assert.IsFalse(requestHeaders["x-random"].hasValue("abc", false));
        }
    }
}
