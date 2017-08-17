using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway
{
    public static class Settings
    {
        public static string RethinkDatabaseHost
        {
            get
            {
                return ConfigurationManager.AppSettings["rethink:host"] ?? "localhost";
            }
        }

        public static int RethinkDatabasePort
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["rethink:host-port"] ?? "28015");
            }
        }

        public static string RethinkDatabaseName
        {
            get
            {
                return ConfigurationManager.AppSettings["rethink:database-name"] ?? "httpgateway";
            }
        }

    }
}
