using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway.Configuration
{
    public class RouteInfo
    {
        readonly UriTemplate _uriTemplate;

        const string _baseAddress = "http://localhost/";

        public string HttpMethod { get; private set; }

        public string RouteTemplate { get; private set; }

        public RouteInfo(string httpVerb, string routeTemplate)
        {
            if (string.IsNullOrWhiteSpace(httpVerb))
            {
                throw new ArgumentNullException(nameof(httpVerb));
            }

            if (string.IsNullOrWhiteSpace(routeTemplate))
            {
                throw new ArgumentNullException(nameof(routeTemplate));
            }

            HttpMethod = httpVerb;
            RouteTemplate = routeTemplate;
            _uriTemplate = new UriTemplate(routeTemplate, true);
        }

        public bool Match(string httpMethod, string path)
        {
            if (
                string.Equals(httpMethod, HttpMethod, StringComparison.InvariantCultureIgnoreCase) |
                string.Equals(httpMethod, "options", StringComparison.InvariantCultureIgnoreCase))
            {
                var trimmedAddress = path.TrimStart('/');

                return _uriTemplate.Match(new Uri(_baseAddress), new Uri($"{_baseAddress}{trimmedAddress}")) != null;
            }

            return false;
        }
    }
}
