using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway.Registries
{
    public class BuiltinServiceRepository : IServiceRepository
    {
        private Connection _connection = null;

        const string SERVICES_TABLE = "services";
        const string INSTANCES_TABLE = "service_instances";

        public BuiltinServiceRepository()
        {
            RethinkDB R = RethinkDB.R;

            _connection = R.Connection()
             .Hostname(Settings.RethinkDatabaseHost)
             .Port(Settings.RethinkDatabasePort)
             .Timeout(60)
             .Connect();
        }

        ~BuiltinServiceRepository()
        {
            Dispose(false);
        }

        public void AddService(Service service)
        {
            if (service == null)
            {
                throw new ArgumentException(nameof(service));
            }

            if (GetService(service.Name, service.Version) == null)
            {
                var x =
                    RethinkDB.R.Db(Settings.RethinkDatabaseName)
                               .Table(SERVICES_TABLE)
                               .Insert(service)
                               .Run<Script>(_connection);
            }
        }

        public void DeleteService(Service service)
        {
            if (service == null)
            {
                throw new ArgumentException(nameof(service));
            }

            if (GetService(service.Name, service.Version) != null)
            {
                var x =
                    RethinkDB.R.Db(Settings.RethinkDatabaseName)
                               .Table(SERVICES_TABLE)
                               .Get(service.id)
                               .Delete()
                               .Run(_connection);
            }
        }

        public void DeleteServiceInstance(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(nameof(id));
            }

            if (GetServiceInstance(id) != null)
            {
                var x =
                    RethinkDB.R.Db(Settings.RethinkDatabaseName)
                               .Table(INSTANCES_TABLE)
                               .Get(id)
                               .Delete()
                               .Run(_connection);
            }
        }

        public Service GetService(string serviceName, string serviceVersion)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                throw new ArgumentException(nameof(serviceName));
            }

            if (string.IsNullOrWhiteSpace(serviceVersion))
            {
                throw new ArgumentException(nameof(serviceVersion));
            }

            return RethinkDB.R.Db(Settings.RethinkDatabaseName)
                              .Table(SERVICES_TABLE)
                              .Get(new Service { Name = serviceName, Version = serviceVersion }.id)
                              .Run<Service>(_connection);
        }

        public ServiceInstance GetServiceInstance(string instanceId)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
            {
                throw new ArgumentException(nameof(instanceId));
            }

            return RethinkDB.R.Db(Settings.RethinkDatabaseName)
                              .Table(INSTANCES_TABLE)
                              .Get(instanceId)
                              .Run<ServiceInstance>(_connection);
        }

        public List<ServiceInstance> GetServiceInstances()
        {
            Cursor<ServiceInstance> query = RethinkDB.R.Db(Settings.RethinkDatabaseName)
                              .Table(INSTANCES_TABLE)
                              //                             .GetAll()
                              .Run<ServiceInstance>(_connection);

            return query.ToList();
        }

        public List<ServiceInstance> GetServiceInstances(Service service)
        {
            Cursor<ServiceInstance> query = RethinkDB.R.Db(Settings.RethinkDatabaseName)
                             .Table(INSTANCES_TABLE)
                             .Filter(x => x["Service"]["Name"].Eq(service.Name) & x["Service"]["Version"].Eq(service.Version))
                             .Run<ServiceInstance>(_connection);

            return query.ToList();
        }

        public List<Service> GetServices()
        {
            Cursor<Service> services = RethinkDB.R.Db(Settings.RethinkDatabaseName)
                             .Table(SERVICES_TABLE)
                             //.GetAll()
                             .Run<Service>(_connection);

            return services.ToList();
        }

        public void SaveServiceInstance(ServiceInstance instance)
        {
            var service = GetService(instance.Service.Name, instance.Service.Version);

            if (service == null)
            {
                AddService(instance.Service);
            }

            if (GetServiceInstance(instance.id) != null)
            {
                RethinkDB.R.Db(Settings.RethinkDatabaseName)
                               .Table(INSTANCES_TABLE)
                               .Get(instance.id)
                               .Update(instance)
                               .Run<Script>(_connection);

            }
            else
            {
                var x =
                    RethinkDB.R.Db(Settings.RethinkDatabaseName)
                               .Table(INSTANCES_TABLE)
                               .Insert(instance)
                               .Run<Script>(_connection);
            }
        }

        public string GetLatestVersionOfService(string serviceName)
        {
            var services = GetServices();

            var versions = from s in services
                           orderby s.Version descending
                           where string.Equals(serviceName, s.Name, StringComparison.InvariantCultureIgnoreCase)
                           select s.Version;

            return versions.FirstOrDefault();
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
