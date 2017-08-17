using Http.Gateway.Registries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Http.Gateway.Host.Controllers
{
    [RoutePrefix("services")]
    public class ServicesController : ApiController
    {

        public BuiltinServiceRepository Registry { get; set; }

        public ServicesController()
            : this(new BuiltinServiceRepository())
        { }

        public ServicesController(BuiltinServiceRepository registry)
        {
            this.Registry = registry;
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult SearchServices()
        {
            return Ok(Registry.GetServices());
        }

        [HttpPut]
        [Route("")]
        public IHttpActionResult AddService([FromBody]Service service)
        {
            Registry.AddService(service);

            return CreatedAtRoute<Service>(Routes.GetService, new { name = service.Name, Version = service.Version }, service);

        }

        [HttpGet]
        [Route("{name}/{version}", Name = Routes.GetService)]
        public IHttpActionResult GetService(string name, string version)
        {
            var service = Registry.GetService(name, version);
            if (service == null)
            {
                return NotFound();
            }

            return Ok(service);
        }

        [HttpDelete]
        [Route("{name}/{version}", Name = Routes.DeleteService)]
        public IHttpActionResult DeleteService(string name, string version)
        {
            var service = Registry.GetService(name, version);
            if (service == null)
            {
                return NotFound();
            }

            Registry.DeleteService(new Service { Name = name, Version = version });

            return Ok();
        }
    }
}