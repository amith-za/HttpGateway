using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using RethinkDb.Driver.Ast;

namespace Http.Gateway
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            InitializeDB();
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        void InitializeDB()
        {
            var db = Settings.RethinkDatabaseName;

            var connection = RethinkDB.R.Connection().Hostname(Settings.RethinkDatabaseHost).Port(Settings.RethinkDatabasePort).Connect();
            var dblist = RethinkDB.R.DbList().Run(connection);

            bool found = false;
            foreach (var item in dblist)
            {
                if (string.Equals(db, item.ToString(), StringComparison.InvariantCulture))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                RethinkDB.R.DbCreate(db).Run(connection);
            }

            foreach (var table in new[] { "basescripts", "scripts", "service_instances", "services", "servicescripts" })
            {
                InitializeTable(db, table, connection);
            }
        }

        static void InitializeTable(string db, string table, Connection connection)
        {
            var tableList = RethinkDB.R.Db(db).TableList().Run(connection);

            bool found = false;
            foreach (var item in tableList)
            {
                if (string.Equals(table, item.ToString(), StringComparison.InvariantCulture))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                RethinkDB.R.Db(db).TableCreate(table).OptArg("primary_key", "id").Run(connection);
            }
        }
    }
}
