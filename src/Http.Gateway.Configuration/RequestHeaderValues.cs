using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway.Configuration
{
    public class RequestHeaderValues
    {
        private IEnumerable<string> _values;

        public RequestHeaderValues()
        {
            _values = new string[0];
        }

        public RequestHeaderValues(IEnumerable<string> values)
        {
            _values = values;
        }

        public bool hasValue(string value, bool caseInsentive = true)
        {
            var comparer = caseInsentive ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture;
            return _values.Contains(value, comparer);
        }
    }
}
