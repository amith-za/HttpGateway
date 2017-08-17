using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;

namespace SimpleMonitor
{
    class Program
    {
        bool _stop = false;
        Thread _monitorThread = null;
        string RegistryUrl = null;
        HttpClient _client = new HttpClient();

        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<Program>(s =>
                {
                    s.ConstructUsing(hostSettings => new Program());
                    s.WhenStarted(hostControl => hostControl.Start());
                    s.WhenStopped(hostControl => hostControl.Stop());
                });

                x.SetDescription("SimpleMonitor");
                x.SetDisplayName("SimpleMonitor");
                x.SetServiceName("SimpleMonitor");

                x.RunAsLocalSystem();
                x.StartAutomatically();
            });
        }

        public Program()
        {
            RegistryUrl = System.Configuration.ConfigurationManager.AppSettings["gateway:url"] ?? "http://localhost/gateway/";
        }

        private void Stop()
        {
            _stop = true;

            for (int i = 0; i < 10; i++)
            {
                if (_monitorThread.IsAlive)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    return;
                }
            }

            _monitorThread.Abort();
        }

        private void Start()
        {
            _stop = false;
            _monitorThread = new Thread(Monitor);
            _monitorThread.Start();
        }

        private void Monitor()
        {
            while (!_stop)
            {
                try
                {
                    var instances = GetServiceInstances();

                    if (instances != null)
                    {
                        foreach (var instance in instances)
                        {
                            if (IsHealthy(instance))
                            {
                                EnsureServiceIsAvailable(instance.Id);
                            }
                            else
                            {
                                UpdateServiceToCritical(instance.Id);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Don't stop believing
                }
                finally
                {
                    Thread.Sleep(5000);
                }
            }
        }

        private bool IsHealthy(ServiceInstance instance)
        {
            try
            {
                var url = $"{instance.Path}/health";

                var response = _client.GetAsync(url);

                if (response.Wait(20000))
                {
                    return response.Result.StatusCode == System.Net.HttpStatusCode.OK;
                }
            }
            catch (Exception)
            { }

            return false;
        }

        public List<ServiceInstance> GetServiceInstances()
        {
            var client = new HttpClient();

            var url = $"{RegistryUrl}/service-instances/";

            var response = client.GetAsync(url).Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<ServiceInstance>>(response.Content.ReadAsStringAsync().Result);
            }

            return null;
        }

        private void EnsureServiceIsAvailable(string id)
        {
            var instance = GetInstance(id);

            if (!string.Equals(instance.Status.ToString(), "Active", StringComparison.InvariantCultureIgnoreCase))
            {
                UpdateInstanceStatus(id, "Active");
            }
        }

        private void UpdateServiceToCritical(string id)
        {
            UpdateInstanceStatus(id, "Critical");
        }

        ServiceInstance GetInstance(string id)
        {
            var client = new HttpClient();

            var url = $"{RegistryUrl}/service-instances/{id}";

            var response = client.GetAsync(url).Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceInstance>(response.Content.ReadAsStringAsync().Result);
            }

            return null;
        }

        private bool UpdateInstanceStatus(string id, string status)
        {
            var client = new HttpClient();

            var url = $"{RegistryUrl}/service-instances/{id}";

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), url);

            var json = JsonConvert.SerializeObject(new { status = status });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            request.Content = content;

            var response = client.SendAsync(request).Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }

            return false;
        }

        public class ServiceInstance
        {
            public string Id { get; set; }

            public string Path { get; set; }

            public string Status { get; set; }
        }
    }
}
