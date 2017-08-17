using Http.Gateway.Registries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Http.Gateway.Host.Controllers
{
    [RoutePrefix("service-instances")]
    public class ServiceInstancesController : ApiController
    {
        public BuiltinServiceRepository Registry { get; set; }

        public ServiceInstancesController()
            : this(new BuiltinServiceRepository())
        { }

        public ServiceInstancesController(BuiltinServiceRepository registry)
        {
            this.Registry = registry;
        }

        [HttpGet]
        [Route("~/services/{name}/{version}/instances")]
        public IHttpActionResult GetInstancesForService(string name, string version)
        {
            var instances = Registry.GetServiceInstances(new Service { Name = name, Version = version });

            return Ok(instances);
        }

        [HttpPut]
        [Route("")]
        public IHttpActionResult RegisterInstance([FromBody] ServiceInstance instance)
        {
            if (!instance.IsValid())
            {
                return BadRequest();
            }

            instance.Status = InstanceStatus.Unavailable;

            Registry.SaveServiceInstance(instance);

            return CreatedAtRoute<ServiceInstance>(
                        Routes.ServiceInstanceGet,
                        new
                        {
                            id = instance.id
                        }, instance);
        }

        [HttpGet]
        [Route("{id}", Name = "ServiceInstance.Get")]
        public IHttpActionResult GetInstance(string id)
        {
            var instance = Registry.GetServiceInstance(id);

            if (instance == null)
            {
                return NotFound();
            }

            return Ok(instance);
        }

        [HttpGet]
        [Route("", Name = "ServiceInstance.GetAll")]
        public IHttpActionResult GetAllInstances()
        {
            var instances = Registry.GetServiceInstances();

            return Ok(instances);
        }

        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult DeregisterInstance(string id)
        {
            Registry.DeleteServiceInstance(id);

            return Ok();
        }

        [HttpPatch]
        [Route("{id}")]
        public IHttpActionResult PatchInstance(string id, [FromBody]Newtonsoft.Json.Linq.JObject requestBody)
        {
            var instance = Registry.GetServiceInstance(id);

            if (instance == null)
            {
                return NotFound();
            }

            dynamic request = requestBody;

            if (request.status != null &&
                Enum.IsDefined(typeof(InstanceStatus), request.status.ToString()))
            {
                instance.Status = (InstanceStatus)Enum.Parse(typeof(InstanceStatus), request.status.ToString());
            }

            Registry.SaveServiceInstance(instance);

            return Ok();
        }
    }

    public static class Routes
    {
        public const string ServiceInstanceGet = "ServiceInstance.Get";
        public const string GetService = "Service.Get";
        public const string DeleteService = "Service.Delete";
    }

    public static class ServiceInstanceExtension
    {
        public static bool IsValid(this ServiceInstance instance)
        {
            var valid = true;

            if (instance == null)
            {
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(instance?.Service?.Name) |
                string.IsNullOrWhiteSpace(instance?.Service?.Version))
            {
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(instance?.id))
            {
                valid = false;
            }

            if (instance?.Path == null)
            {
                valid = false;
            }

            return valid;
        }
    }
}