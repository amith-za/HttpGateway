using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Http.Gateway.Modules
{
    public class RetryModule : Module
    {

        const string RETRY_COUNT_ROUTING_PROPERTY = "retry_count";

        public int RetryCount { get; private set; }

        public int[] RetryStatusCodes { get; set; }

        public override void Configure(ConfigurationMap configurationObject)
        {
            RetryCount = Convert.ToInt32(configurationObject.GetValueOrDefault("retry_count", "0"));
            RetryStatusCodes = ((string[])configurationObject.GetValueOrDefault("status_codes", new string[] { })).Select(x => Convert.ToInt32(x)).ToArray();
        }

        public override void Initialize(ConfigurationMap configurationObject)
        {

        }

        public override async Task<HttpResponseMessage> Handle(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var retryCount = 0;
            if (request.RoutingPropertyExists(RETRY_COUNT_ROUTING_PROPERTY))
            {
                retryCount = Convert.ToInt32(request.GetRoutingProperty(RETRY_COUNT_ROUTING_PROPERTY));
                retryCount++;
            }
            request.AddUpdateRoutingProperty(RETRY_COUNT_ROUTING_PROPERTY, (retryCount).ToString());

            // RequestMessage is unusable after first retry
            if (retryCount > 0)
            {
                request = await CloneHttpRequestMessageAsync(request);
            }

            var response
                = await base.Handle(request, cancellationToken).ContinueWith
                (
                async x =>
                {
                    var requestFailed = false;

                    HttpResponseMessage result = null;

                    if (x.Exception != null)
                    {
                        if (x.Exception?.InnerException?.GetType() == typeof(HttpRequestException))
                        {
                            result = request.CreateResponse(System.Net.HttpStatusCode.ServiceUnavailable);
                            requestFailed = true;
                        }
                    }
                    else
                    {
                        result = x.Result;

                        if (result.StatusCode == System.Net.HttpStatusCode.BadGateway |
                            result.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                        {
                            requestFailed = true;
                        }
                    }

                    if (requestFailed &
                        Convert.ToInt32(request.GetRoutingProperty(RETRY_COUNT_ROUTING_PROPERTY)) < RetryCount)
                    {
                        result = await Handle(request, cancellationToken);
                    }

                    if (result.Headers.Contains("MCA-RETRY"))
                    {
                        result.Headers.Remove("MCA-RETRY");
                    }

                    result.Headers.Add("MCA-RETRY", request.GetRoutingProperty(RETRY_COUNT_ROUTING_PROPERTY));

                    return result;
                });

            return await response;
        }

        private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
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

            return clone;
        }
    }
}
