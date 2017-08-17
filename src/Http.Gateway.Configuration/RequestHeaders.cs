using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway.Configuration
{
    public class RequestHeaders
    {
        private HttpRequestHeaders _headers = null;

        public RequestHeaders(HttpRequestHeaders headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }
            _headers = headers;
        }

        public bool hasHeader(string header)
        {
            return _headers.Contains(header);
        }

        public RequestHeaderValues this[string key]
        {
            get
            {
                return hasHeader(key)
                       ? new RequestHeaderValues(_headers.GetValues(key))
                       : new RequestHeaderValues();

            }
        }
    }
}
