using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Http.Gateway.Modules
{
    public class UserDataToJWTModule : Module
    {
        public class JwtToken
        {
            public DateTime? _expiryTime;

            public string accessToken { get; set; }

            public string tokenType { get; set; }

            public long sseExpiresAt { get; set; }

            public DateTime expiryTime
            {
                get
                {
                    if (!_expiryTime.HasValue)
                    {
                        _expiryTime = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(sseExpiresAt);
                    }

                    return _expiryTime.Value;
                }
            }

            public bool IsExpired
            {
                get
                {
                    return expiryTime < DateTime.Now;
                }
            }
        }

        private static ConcurrentDictionary<string, JwtToken> userDataCache = new ConcurrentDictionary<string, JwtToken>();

        private JwtToken _gatewayIdentityToken;

        public string UserApiEndpoint { get; private set; }

        public override void Configure(ConfigurationMap configurationObject)
        {

        }

        public override void Initialize(ConfigurationMap configurationObject)
        {
            if (configurationObject.ContainsKey("user_endpoint"))
            {
                UserApiEndpoint = (string)configurationObject["user_endpoint"];
            }
            else
            {
                throw new Exception("user_endpoint configuration property is required");
            }
        }

        public override Task<HttpResponseMessage> Handle(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                object property = null;

                if (request.Properties.TryGetValue(UserData.USER_IDENTITY_DATA_KEY, out property))
                {
                    var userdata = property as UserData;

                    if (userdata != null)
                    {
                        JwtToken token = GetUserApiToken(userdata.Country, userdata.UserName);

                        var userApiToken = token?.accessToken;
                        if (!string.IsNullOrWhiteSpace(userApiToken))
                        {
                            request.Headers.Add("authorization", $"bearer {userApiToken}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }

            return base.Handle(request, cancellationToken);
        }

        private JwtToken GetUserApiToken(string country, string userName)
        {
            var key = $"{country}:{userName}";
            try
            {
                if (!userDataCache.ContainsKey(key) ||
                    userDataCache[key].IsExpired)
                {
                    using (var client = new HttpClient())
                    {
                        var json = JsonConvert.SerializeObject(
                            new
                            {
                                token = GetGatewayApiToken(country)?.accessToken,
                                userName = userName
                            });
                        var payload = new StringContent(json, Encoding.UTF8, "application/json");

                        var response = client.PostAsync($"{UserApiEndpoint}/{country}/users/access-tokens/token", payload).Result;

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var token = JsonConvert.DeserializeObject<JwtToken>(response.Content.ReadAsStringAsync().Result);

                            userDataCache.AddOrUpdate(key, token, (k, v) => token);
                        }
                    }
                }

            }
            catch (Exception e)
            {
            }

            return userDataCache.ContainsKey(key) ? userDataCache[key] : null;
        }

        private JwtToken GetGatewayApiToken(string country)
        {
            if (_gatewayIdentityToken == null ||
               _gatewayIdentityToken.expiryTime < DateTime.UtcNow)
            {
                var httpClientHandler = new HttpClientHandler()
                {
                    UseDefaultCredentials = true,
                    UseProxy = false
                };
                using (var client = new HttpClient(httpClientHandler))
                {
                    var response = client.PostAsync($"{UserApiEndpoint}/{country}/users/access-tokens/windows", null).Result;

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var token = JsonConvert.DeserializeObject<JwtToken>(response.Content.ReadAsStringAsync().Result);
                        _gatewayIdentityToken = token;
                    }
                }
            }

            return _gatewayIdentityToken;
        }
    }
}
