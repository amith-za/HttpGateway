using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway
{
    public class ScriptStorage : IDisposable
    {
        private Connection _connection = null;

        public ScriptStorage()
        {
            RethinkDB R = RethinkDB.R;

            _connection = R.Connection()
             .Hostname(Settings.RethinkDatabaseHost)
             .Port(Settings.RethinkDatabasePort)
             .Timeout(60)
             .Connect();
        }

        ~ScriptStorage()
        {
            Dispose(false);
        }

        public Script GetModulesScript()
        {
            Script x =
                RethinkDB.R.Db(Settings.RethinkDatabaseName)
                           .Table("basescripts")
                           .Get("_modules")
                           .Run<Script>(_connection);

            return x;
        }

        public void SaveModulesScript(string modulesScript)
        {
            UpdateScript(modulesScript, "_modules", Settings.RethinkDatabaseName, "basescripts");
        }

        public Script GetRegistriesScript()
        {
            return GetScript("_registries", Settings.RethinkDatabaseName, "basescripts");
        }

        public void SaveRegistriesScript(string registriesScript)
        {
            UpdateScript(registriesScript, "_registries", Settings.RethinkDatabaseName, "basescripts");
        }

        public Script GetServiceScriptsById(string scriptId)
        {
            return GetScript(scriptId, Settings.RethinkDatabaseName, "servicescripts");
        }

        public void SaveServiceScript(string id, string serviceScript)
        {
            UpdateScript(serviceScript, id, Settings.RethinkDatabaseName, "servicescripts");
        }

        public List<Script> GetServiceScripts(string nameFilter = null, int page = 1, int pageSize = 20)
        {
            if (page < 1)
            {
                page = 1;
            }

            var skip = pageSize * (page - 1);

            Cursor<Script> query = null;

            if (string.IsNullOrWhiteSpace(nameFilter))
            {
                query = RethinkDB.R.Db(Settings.RethinkDatabaseName)
                    .Table("servicescripts")
                    .Skip(skip)
                    .Limit(pageSize)
                    .Run<Script>(_connection);
            }
            else
            {
                query = RethinkDB.R.Db(Settings.RethinkDatabaseName)
                    .Table("servicescripts")
                    .Filter(x => x["id"].Match($"^{nameFilter}"))
                    .Skip(skip)
                    .Limit(pageSize)
                    .Run<Script>(_connection);
            }

            return query.ToList();
        }

        public void DeleteServiceScript(string scriptId)
        {
            RethinkDB.R
                .Db(Settings.RethinkDatabaseName)
                .Table("servicescripts")
                .Get(scriptId)
                .Delete()
                .Run(_connection);
        }

        private void UpdateScript(string script, string id, string database, string table)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(script))
            {
                throw new ArgumentNullException(nameof(script));
            }

            if (GetScript(id, database, table) != null)
            {
                RethinkDB.R.Db(database)
                               .Table(table)
                               .Get(id)
                               .Update(new { content = script })
                               .Run<Script>(_connection);

            }
            else
            {
                var newScript = new Script
                {
                    id = id,
                    content = script
                };

                var x =
                    RethinkDB.R.Db(database)
                               .Table(table)
                               .Insert(newScript)
                               .Run<Script>(_connection);
            }
        }

        private Script GetScript(string id, string database, string table)
        {
            Script x =
            RethinkDB.R.Db(database)
                       .Table(table)
                       .Get(id)
                       .Run<Script>(_connection);

            return x;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            _connection = null;
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }
    }
}
