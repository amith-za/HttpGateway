using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway
{
    public class UserData
    {
        public const string USER_IDENTITY_DATA_KEY = "user-identity-data";

        public UserData(string country, string username)
        {
            Country = country;
            UserName = username;
        }

        public string Country { get; private set; }

        public string UserName { get; private set; }
    }
}
