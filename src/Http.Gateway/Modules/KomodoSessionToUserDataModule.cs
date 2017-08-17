using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Http.Gateway.Modules
{
    public class KomodoSessionToUserDataModule : Module
    {
        class CachedUserData
        {
            public string SessionId { get; set; }

            public DateTime TokenExpiryTime { get; set; }

            public bool HasSessionExpired
            {
                get
                {
                    return DateTime.Now > TokenExpiryTime;
                }
            }

            public UserData UserData { get; set; }
        }

        private static ConcurrentDictionary<string, CachedUserData> userDataCache = new ConcurrentDictionary<string, CachedUserData>();

        public string HeaderName { get; private set; }

        public string TokenUrl { get; private set; }

        public override void Configure(ConfigurationMap configurationObject)
        {
        }

        public override void Initialize(ConfigurationMap configurationObject)
        {
            if (configurationObject.ContainsKey("header_name"))
            {
                HeaderName = (string)configurationObject["header_name"];
            }
            else
            {
                HeaderName = "komodo-sessiontoken";
            }

            if (configurationObject.ContainsKey("token_url"))
            {
                TokenUrl = (string)configurationObject["token_url"];
            }
            else
            {
                throw new Exception("token_url configuration property is required");
            }
        }

        public override Task<HttpResponseMessage> Handle(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var sessionId = (from header in request.Headers
                                 where header.Key == HeaderName
                                 select header.Value.FirstOrDefault()).FirstOrDefault();

                var authorization = (from header in request.Headers
                                     where header.Key == "authorization"
                                     select header.Value.FirstOrDefault()).FirstOrDefault();

                if (!request.Properties.ContainsKey(UserData.USER_IDENTITY_DATA_KEY) &
                    !string.IsNullOrWhiteSpace(sessionId) &
                    string.IsNullOrWhiteSpace(authorization))
                {
                    if (!userDataCache.ContainsKey(sessionId))
                    {

                        var data = GetUserDataForSessionId(sessionId);

                        if (data != null)
                        {
                            userDataCache.AddOrUpdate(sessionId, data, (k, v) => data);

                            // Should move this to a background thread
                            var oldEntries = from s in userDataCache.Values
                                             where s.UserData.UserName == data.UserData.UserName &
                                                   s.SessionId != data.SessionId
                                             select s.SessionId;

                            foreach (var item in oldEntries)
                            {
                                CachedUserData remove;
                                userDataCache.TryRemove(item, out remove);
                            }
                        }
                    }

                    CachedUserData userdata;
                    if (userDataCache.TryGetValue(sessionId, out userdata))
                    {
                        request.Properties.Add(UserData.USER_IDENTITY_DATA_KEY, userdata.UserData);
                    }
                }
            }
            catch (Exception e)
            {
            }

            return base.Handle(request, cancellationToken);
        }

        private CachedUserData GetUserDataForSessionId(string sessionId)
        {
            CachedUserData userData = null;

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = null;
                    response = client.GetAsync(TokenUrl + sessionId).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content.ReadAsStringAsync().Result;
                        JObject token = null;

                        token = JObject.Parse(responseContent);

                        var identityTokenExpiryDate = token["identityTokenExpiryDate"].CreateReader().ReadAsDateTimeOffset();
                        if (identityTokenExpiryDate != null)
                        {
                            var dateCreated = token["dateCreated"].CreateReader().ReadAsDateTimeOffset();
                            if (dateCreated != null)
                            {
                                var userName = token["userName"].ToString();
                                var country = token["country"].ToString();

                                userData = new CachedUserData
                                {
                                    SessionId = sessionId,
                                    TokenExpiryTime = DateTime.Parse(identityTokenExpiryDate.ToString()),
                                    UserData = new UserData(country, userName)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // if there are any failures for networking etc don't bubble it up
            }

            return userData;
        }
    }
}
