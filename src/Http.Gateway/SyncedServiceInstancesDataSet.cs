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
    public class SyncedServiceInstancesDataSet
    {
        private RethinkDB _R = RethinkDB.R;
        private IDisposable _subscription = null;
        private ImmutableDictionary<string, ServiceInstance> _instances = null;
        private ImmutableDictionary<string, ServiceInstance> _initial = null;
        private bool _initializing = false;

        public string DataBase { get; private set; }
        public string Table { get; private set; }
        public Connection.Builder ConnectionBuilder { get; private set; }

        public bool Ready
        {
            get
            {
                return _instances != null;
            }
        }

        public SyncedServiceInstancesDataSet(string dataBase, string table, Connection.Builder connectionBuilder)
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

            _instances = ImmutableDictionary<string, ServiceInstance>.Empty;
        }

        public List<ServiceInstance> GetActiveServiceInstances(string service, string version)
        {
            var instances =
                   from
                    s in _instances.Values
                   where
                    string.Equals(s.Service.Name, service, StringComparison.InvariantCultureIgnoreCase) &
                    string.Equals(s.Service.Version, version, StringComparison.InvariantCultureIgnoreCase) &
                    s.Status == InstanceStatus.Active
                   select s;

            return instances.ToList();
        }

        public void StartSync()
        {
            StopSync();

            var _connection = ConnectionBuilder.Connect();

            var changes =
                _R.Db(DataBase)
                .Table(Table)
                .Changes()[new { include_states = true, include_initial = true }]
                .RunChanges<ServiceInstance>(_connection);
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

        protected virtual void AddInstance(ServiceInstance serviceInstance)
        {
            if (!_initializing)
            {
                if (_instances.ContainsKey(serviceInstance.id))
                {
                    _instances = _instances.SetItem(serviceInstance.id, serviceInstance);
                }
                else
                {
                    _instances = _instances.Add(serviceInstance.id, serviceInstance);
                }
            }
            else
            {
                _initial = _initial.Add(serviceInstance.id, serviceInstance);
            }
        }

        protected virtual void RemoveInstance(ServiceInstance script)
        {
            if (_instances.ContainsKey(script.id))
            {
                _instances = _instances.Remove(script.id);
            }
        }

        private void OnNext(RethinkDb.Driver.Model.Change<ServiceInstance> change)
        {
            if (change.State == RethinkDb.Driver.Model.ChangeState.Initializing)
            {
                _initializing = true;
                _initial = ImmutableDictionary<string, ServiceInstance>.Empty;
                return;
            }

            if (change.State == RethinkDb.Driver.Model.ChangeState.Ready)
            {
                _initializing = false;
                _instances = _initial;
            }

            if (change.State == null)
            {
                if (change?.NewValue == null)
                {
                    RemoveInstance(change.OldValue);
                }
                else
                {
                    AddInstance(change.NewValue);
                }
            }
        }
    }
}
