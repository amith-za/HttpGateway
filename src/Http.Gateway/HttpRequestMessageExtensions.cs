using Http.Gateway.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway
{
    public static class HttpRequestMessageExtensions
    {
        public static async Task<HttpRequestMessage> Clone(this HttpRequestMessage req)
        {
            HttpRequestMessage clone = new HttpRequestMessage(req.Method, req.RequestUri);

            // Copy the request's content (via a MemoryStream) into the cloned object
            var ms = new MemoryStream();
            if (req.Content != null)
            {
                await req.Content.CopyToAsync(ms).ConfigureAwait(false);
                ms.Position = 0;
                clone.Content = new StreamContent(ms);

                // Copy the content headers
                if (req.Content.Headers != null)
                    foreach (var h in req.Content.Headers)
                        clone.Content.Headers.Add(h.Key, h.Value);
            }

            clone.Version = req.Version;

            foreach (KeyValuePair<string, object> prop in req.Properties)
                clone.Properties.Add(prop);

            foreach (KeyValuePair<string, IEnumerable<string>> header in req.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            if (clone.Method == HttpMethod.Get)
            {
                clone.Content = null;
            }

            return clone;
        }

        public static string NormalizedPath(this HttpRequestMessage request)
        {
            var virtualDirectory = request.GetConfiguration().VirtualPathRoot;

            var normalizedPath = request.RequestUri.PathAndQuery;
            normalizedPath = normalizedPath.Substring(virtualDirectory.Length, normalizedPath.Length - virtualDirectory.Length);

            return normalizedPath;
        }

        public static bool RoutingPropertyExists(this HttpRequestMessage request, string key)
        {
            var requestRoutingProperties = GetRequestRoutingProperties(request);

            return
                requestRoutingProperties != null
                ? requestRoutingProperties.ContainsKey(key)
                : false;
        }

        public static string GetRoutingProperty(this HttpRequestMessage request, string key)
        {
            var requestRoutingDetails = GetRequestRoutingProperties(request);

            return
                requestRoutingDetails.ContainsKey(key)
                ? requestRoutingDetails[key]
                : null;
        }

        public static void AddUpdateRoutingProperty(this HttpRequestMessage request, string key, string value)
        {
            var requestRoutingDetails = GetRequestRoutingProperties(request);

            if (!requestRoutingDetails.ContainsKey(key))
            {
                requestRoutingDetails.Add(key, value);
            }
            else
            {
                requestRoutingDetails[key] = value;
            }
        }

        private static RequestRoutingProperties GetRequestRoutingProperties(HttpRequestMessage request)
        {
            return
                request.Properties.ContainsKey(RequestRoutingProperties.REQUEST_PROPERTY_KEY)
                ? request.Properties[RequestRoutingProperties.REQUEST_PROPERTY_KEY] as RequestRoutingProperties
                : null;
        }
    }
}
