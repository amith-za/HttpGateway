using Http.Gateway.Configuration.Dsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Http.Gateway.Host.Controllers
{
    [RoutePrefix("configuration-scripts")]
    public class ConfigurationController : ApiController
    {
        public ScriptStorage ScriptStorage { get; private set; }

        public IModuleCatalog ModuleCatalog { get; private set; }

        public IRegistryCatalog RegistryCatalog { get; private set; }

        public ConfigurationController()
            : this(new ScriptStorage(), new SyncedModuleCatalog(), new SyncedRegistryCatalog())
        { }

        public ConfigurationController(ScriptStorage scriptStorage, IModuleCatalog moduleCatalog, IRegistryCatalog registryCatalog)
        {
            if (scriptStorage == null)
            {
                throw new ArgumentNullException(nameof(scriptStorage));
            }

            if (moduleCatalog == null)
            {
                throw new ArgumentNullException(nameof(moduleCatalog));
            }

            if (registryCatalog == null)
            {
                throw new ArgumentNullException(nameof(registryCatalog));
            }

            ScriptStorage = scriptStorage;
            ModuleCatalog = moduleCatalog;
            RegistryCatalog = registryCatalog;
        }

        [Route("modules")]
        [HttpGet]
        public IHttpActionResult GetModules()
        {
            var script = ScriptStorage.GetModulesScript();

            if (script == null)
            {
                return NotFound();
            }

            return Ok(script);
        }

        [Route("modules")]
        [HttpPost]
        public IHttpActionResult UpdateModules(UpdateScriptRequest request)
        {
            var parser = new Http.Gateway.Configuration.Dsl.ModuleParser();
            var parseResult = parser.ParseModules(request.Script);

            if (parseResult.HasErrors)
            {
                return Content(HttpStatusCode.BadRequest, parseResult.ParserMessages.Select(t => new { level = t.Level.ToString().ToLowerInvariant(), location = t.Location, message = t.Message }));
            }

            ScriptStorage.SaveModulesScript(request.Script);

            return Ok();
        }

        [Route("registries")]
        [HttpGet]
        public IHttpActionResult GetRegistries()
        {
            var script = ScriptStorage.GetRegistriesScript();

            if (script == null)
            {
                return NotFound();
            }

            return Ok(script);
        }

        [Route("registries")]
        [HttpPost]
        public IHttpActionResult UpdateRegistries(UpdateScriptRequest request)
        {
            var parser = new Http.Gateway.Configuration.Dsl.RegistryParser();
            var parseResult = parser.ParseRegistries(request.Script);

            if (parseResult.HasErrors)
            {
                return Content(HttpStatusCode.BadRequest, parseResult.ParserMessages.Select(t => new { level = t.Level.ToString().ToLowerInvariant(), location = t.Location, message = t.Message }));
            }

            ScriptStorage.SaveRegistriesScript(request.Script);

            return Ok();
        }

        [Route("services")]
        [HttpGet]
        public IHttpActionResult GetServiceScripts(string nameFilter = null, int page = 1, int pageSize = 20)
        {
            return Ok(ScriptStorage.GetServiceScripts(nameFilter, page, pageSize));
        }

        [Route("services/{serviceName}")]
        [HttpGet]
        public IHttpActionResult GetServiceScript(string serviceName)
        {
            var script = ScriptStorage.GetServiceScriptsById(serviceName);

            if (script == null)
            {
                return NotFound();
            }

            return Ok(script);
        }

        [Route("services/{serviceName}")]
        [HttpPut]
        public IHttpActionResult SaveServiceScript(string serviceName, UpdateScriptRequest request)
        {
            var registryNames = RegistryCatalog.GetRegistryDefinitions().Select(r => r.Id).ToList();

            var parser = new RoutingDefinitionParser(registryNames, ModuleCatalog.GetAllowedModuleNames());
            var parseResult = parser.ParseRoutingDefinition(request.Script);

            if (parseResult.HasErrors)
            {
                return Content(HttpStatusCode.BadRequest, parseResult.ParserMessages.Select(t => new { level = t.Level.ToString().ToLowerInvariant(), location = t.Location, message = t.Message }));
            }

            ScriptStorage.SaveServiceScript(serviceName, request.Script);

            return Ok();
        }

        [Route("services/{serviceName}")]
        [HttpDelete]
        public IHttpActionResult DeleteServiceScript(string serviceName)
        {
            if (ScriptStorage.GetServiceScriptsById(serviceName) == null)
            {
                return NotFound();
            }

            ScriptStorage.DeleteServiceScript(serviceName);

            return Ok();
        }
    }
}
