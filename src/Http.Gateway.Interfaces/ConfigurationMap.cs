using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway
{
    public class ConfigurationMap : Dictionary<string, object>
    {

        public ConfigurationMap()
        {

        }

        public object GetValueOrDefault(string key, object defaultValue = null)
        {
            if (ContainsKey(key))
            {
                return this[key];
            }

            return defaultValue;
        }
    }
}
