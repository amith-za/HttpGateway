using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Http.Gateway
{
    public class SyncedScriptDataSet
    {
        private RethinkDB _R = RethinkDB.R;
        private IDisposable _subscription = null;
        private ImmutableDictionary<string, Script> _scripts = null;
        private ImmutableDictionary<string, Script> _initial = null;
        private bool _initializing = false;

        public string DataBase { get; private set; }
        public string Table { get; private set; }
        public Connection.Builder ConnectionBuilder { get; private set; }

        public bool Ready
        {
            get
            {
                return _scripts != null;
            }
        }

        public SyncedScriptDataSet(string dataBase, string table, Connection.Builder connectionBuilder)
        {
            if (string.IsNullOrWhiteSpace(dataBase))
            {
                throw new ArgumentNullException(nameof(dataBase));
            }

            if (string.IsNullOrWhiteSpace(table))
            {
                throw new ArgumentNullException(nameof(table));
            }

            if (connectionBuilder == null)
            {
                throw new ArgumentNullException(nameof(connectionBuilder));
            }

            DataBase = dataBase;
            Table = table;
            ConnectionBuilder = connectionBuilder;

            _scripts = ImmutableDictionary<string, Script>.Empty;

            Cursor<Script> query = RethinkDB.R.Db(DataBase)
                    .Table(Table)
                    .Run<Script>(ConnectionBuilder.Connect());

            foreach (var item in query)
            {
                _scripts = _scripts.Add(item.id, item);
            }
        }

        public System.Collections.Immutable.ImmutableList<Script> GetScripts()
        {
            return _scripts.Values.ToImmutableList();
        }

        public Script GetScript(string scriptId)
        {
            return _scripts.ContainsKey(scriptId) ? _scripts[scriptId] : null;
        }

        public void StartSync()
        {
            StopSync();

            var _connection = ConnectionBuilder.Connect();

            var changes =
                _R.Db(DataBase)
                .Table(Table)
                .Changes()[new { include_states = true, include_initial = true }]
                .RunChanges<Script>(_connection);
            var observable = changes.ToObservable();

            _subscription =
                observable
                    .SubscribeOn(System.Reactive.Concurrency.NewThreadScheduler.Default)
                    .Subscribe(OnNext, OnError, OnCompleted);
        }

        public void StopSync()
        {
            if (_subscription != null)
            {
                _subscription.Dispose();
            }
        }

        private void OnCompleted()
        {
        }

        private void OnError(Exception obj)
        {
            while (true)
            {
                try
                {
                    StartSync();
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(3000);
                }
            }
        }

        protected virtual void AddScript(Script script)
        {
            if (!_initializing)
            {
                if (_scripts.ContainsKey(script.id))
                {
                    _scripts = _scripts.SetItem(script.id, script);
                }
                else
                {
                    _scripts = _scripts.Add(script.id, script);
                }
            }
            else
            {
                _initial = _initial.Add(script.id, script);
            }
        }

        protected virtual void RemoveScript(Script script)
        {
            if (_scripts.ContainsKey(script.id))
            {
                _scripts = _scripts.Remove(script.id);
            }
        }

        private void OnNext(RethinkDb.Driver.Model.Change<Script> change)
        {
            if (change.State == RethinkDb.Driver.Model.ChangeState.Initializing)
            {
                _initializing = true;
                _initial = ImmutableDictionary<string, Script>.Empty;
                return;
            }

            if (change.State == RethinkDb.Driver.Model.ChangeState.Ready)
            {
                _initializing = false;
                _scripts = _initial;
            }

            if (change.State == null)
            {
                if (change?.NewValue == null)
                {
                    RemoveScript(change.OldValue);
                }
                else
                {
                    AddScript(change.NewValue);
                }
            }
        }
    }
}
