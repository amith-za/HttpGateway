using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RethinkDb.Driver.Net;
using System.Collections.Immutable;
using Http.Gateway.Configuration.Dsl;
using RethinkDb.Driver;

namespace Http.Gateway
{
    public class SyncedServiceScripts : SyncedScriptDataSet
    {
        private ImmutableDictionary<string, RequestPipleline> _pipelines;
        private object lockObj = new object();

        public IModuleCatalog ModuleCatalog { get; private set; }

        public IRegistryCatalog RegistryCatalog { get; private set; }

        public SyncedServiceScripts(Connection.Builder connectionBuilder)
            : this(connectionBuilder, new SyncedModuleCatalog(), new SyncedRegistryCatalog())
        {
        }

        private SyncedServiceScripts(Connection.Builder connectionBuilder, IModuleCatalog moduleCatalog, IRegistryCatalog registryCatalog)
            : base(Settings.RethinkDatabaseName, "servicescripts", connectionBuilder)
        {
            if (moduleCatalog == null)
            {
                throw new ArgumentNullException(nameof(moduleCatalog));
            }

            if (registryCatalog == null)
            {
                throw new ArgumentNullException(nameof(registryCatalog));
            }

            _pipelines = ImmutableDictionary<string, RequestPipleline>.Empty;

            ModuleCatalog = moduleCatalog;
            RegistryCatalog = registryCatalog;

            Cursor<Script> query = RethinkDB.R.Db(DataBase)
                    .Table(Table)
                    .Run<Script>(ConnectionBuilder.Connect());

            foreach (var item in query)
            {
                AddScript(item);
            }
        }

        public IEnumerable<RequestPipleline> GetRequestPipelines()
        {
            return _pipelines.Values.ToImmutableList();
        }

        protected override void AddScript(Script script)
        {
            var parser =
                new RoutingDefinitionParser(
                    RegistryCatalog.GetRegistryDefinitions().Select(r => r.Id).ToList(),
                    ModuleCatalog.GetAllowedModuleNames());

            var pipelineFactory = new PipelineFactory(ModuleCatalog, RegistryCatalog);

            var routingDefinition = parser.ParseRoutingDefinition(script.content);

            if (!routingDefinition.HasErrors)
            {
                var pipeline = pipelineFactory.CreatePipeline(routingDefinition.Result);

                lock (lockObj)
                {
                    if (_pipelines.ContainsKey(script.id))
                    {
                        _pipelines = _pipelines.SetItem(script.id, pipeline);
                    }
                    else
                    {
                        _pipelines = _pipelines.Add(script.id, pipeline);
                    }
                }
            }

            base.AddScript(script);
        }

        protected override void RemoveScript(Script script)
        {
            _pipelines = _pipelines.Remove(script.id);

            base.RemoveScript(script);
        }
    }
}
