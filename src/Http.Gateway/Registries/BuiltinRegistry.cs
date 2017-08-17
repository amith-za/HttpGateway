using RethinkDb.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Http.Gateway.Registries
{
    public class BuiltinRegistry : IRegistry
    {
        private static SyncedServiceInstancesDataSet _serviceInstances = null;

        class RequestCounter
        {
            private int _requestCount;

            public int GetNext()
            {
                return System.Threading.Interlocked.Increment(ref _requestCount);
            }
        }

        private Func<Tuple<string, string>> _serviceSelectionStrategy = null;
        private static Dictionary<string, RequestCounter> _requestCounters = new Dictionary<string, RequestCounter>();
        private static object lockObj = new object();

        public string ServiceName { get; set; }

        public string UseVersion { get; set; }

        public bool UseLatest { get; set; }

        public bool UseStable { get; set; }

        public IServiceRepository ServiceRepository { get; private set; }

        static BuiltinRegistry()
        {
            _serviceInstances
                =
                new SyncedServiceInstancesDataSet(
                    Settings.RethinkDatabaseName,
                    "service_instances",
                    RethinkDB.R.Connection()
                     .Hostname(Settings.RethinkDatabaseHost)
                     .Port(Settings.RethinkDatabasePort)
                     .Timeout(60));

            _serviceInstances.StartSync();
        }

        public BuiltinRegistry()
            : this(new BuiltinServiceRepository())
        {

        }

        public BuiltinRegistry(IServiceRepository serviceRepository)
        {
            if (serviceRepository == null)
            {
                throw new ArgumentNullException(nameof(serviceRepository));
            }

            ServiceRepository = serviceRepository;
        }

        public void Configure(ConfigurationMap configurationObject)
        {
            ServiceName = configurationObject.GetValueOrDefault("service_name") as string;
            UseVersion = configurationObject.GetValueOrDefault("use_version") as string;
            UseLatest = Convert.ToBoolean(configurationObject.GetValueOrDefault("use_latest", "false"));
            UseStable = Convert.ToBoolean(configurationObject.GetValueOrDefault("use_stable", "false"));

            if (string.IsNullOrWhiteSpace(ServiceName))
            {
                throw new Exception("service_name is a required configuration property");
            }

            if (!string.IsNullOrWhiteSpace(UseVersion))
            {
                _serviceSelectionStrategy = SelectSpecificVersion;
            }
            else if (UseStable)
            {
                _serviceSelectionStrategy = SelectLatestStableVersion;
            }
            else if (UseLatest)
            {
                _serviceSelectionStrategy = SelectLatestVersion;
            }
            else
            {
                throw new Exception("No valid options selected, provide either 'use_version'|'use_stable'|'use_latest'");
            }
        }

        public void Initialize(ConfigurationMap configurationObject)
        {
        }

        public Uri Next()
        {
#warning exclude unavailable services

            var service = _serviceSelectionStrategy.Invoke();

            var instances = _serviceInstances.GetActiveServiceInstances(service.Item1, service.Item2);

            if (instances == null || instances.Count < 1)
            {
                return null;
            }

            return instances[GetRequestCount(service.Item1, service.Item2) % instances.Count].Path;
        }

        private int GetRequestCount(string serviceName, string serviceVersion)
        {
            var key = $"{serviceName}:{serviceVersion}";

            if (!_requestCounters.ContainsKey(key))
            {
                lock (lockObj)
                {
                    if (!_requestCounters.ContainsKey(key))
                    {
                        _requestCounters.Add(key, new RequestCounter());
                    }
                }
            }

            return _requestCounters[key].GetNext();
        }

        private Tuple<string, string> SelectSpecificVersion()
        {
            return new Tuple<string, string>(ServiceName, UseVersion);
        }

        private Tuple<string, string> SelectLatestStableVersion()
        {
            return new Tuple<string, string>(ServiceName, UseVersion);
        }

        private Tuple<string, string> SelectLatestVersion()
        {
            return new Tuple<string, string>(ServiceName, ServiceRepository.GetLatestVersionOfService(ServiceName));
        }
    }
}
