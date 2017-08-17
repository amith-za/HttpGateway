using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway.Configuration
{
    public class RequestRoutingProperties : Dictionary<string, string>
    {
        public const string REQUEST_PROPERTY_KEY = "http:gateway:requestroutingdetails";

        public RequestHeaders headers { get; set; }

        public RequestRoutingProperties(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("user-data"))
            {
                var userData = request.Properties["user-data"] as UserData;
                base["user"] = userData.UserName;
                base["country"] = userData.Country;
            }

            headers = new RequestHeaders(request.Headers);
        }

        public new string this[string key]
        {
            get
            {
                return base.ContainsKey(key)
                       ? base[key]
                       : null;
            }
            set
            {
                if (base.ContainsKey(key))
                {
                    base[key] = value;
                }
            }
        }
    }

}
