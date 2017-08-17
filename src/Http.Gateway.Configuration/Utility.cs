using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway.Configuration
{
    public class Utility
    {
        public static Type ScanForType(string type)
        {
            var handlerType = Type.GetType(type);

            if (handlerType == null)
            {
                handlerType = (from a in System.AppDomain.CurrentDomain.GetAssemblies()
                               from t in a.GetTypes()
                               where string.Equals(t.FullName, type)
                               select t).FirstOrDefault();
            }

            return handlerType;
        }
    }
}
