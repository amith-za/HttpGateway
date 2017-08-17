using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway.Configuration
{
    public class RoutingDefinition
    { 
        public List<RouteInfo> Routes { get; set; }

        public List<BackendDefinition> Backends { get; set; }

        public List<RouteModule> RouteModules { get; set; }

        public List<RoutingRule> RoutingRules { get; set; }

        public RoutingDefinition()
        {
            Routes = new List<RouteInfo>();
            Backends = new List<BackendDefinition>();
            RouteModules = new List<RouteModule>();
            RoutingRules = new List<RoutingRule>();
        }

        //public void AddModule(string module)
        //{
        //    RouteModules.Add(module);
        //}

        //public void InsertModule(int index, string module)
        //{
        //    RouteModules.Insert(index, module);
        //}

        //public void AddRoutingRule(RoutingRule module)
        //{
        //    RoutingRules.Add(module);
        //}

        public ConfigurationValidationResult Validate()
        {
            var validationResult = new ConfigurationValidationResult();
            validationResult.IsValid = true;

            RoutingRule defaultRoute = null;
            var backendIds = Backends.Select(b => b.Id);
            foreach (var routingRule in RoutingRules)
            {
                if (routingRule.IsDefault)
                {
                    if (defaultRoute == null)
                    {
                        defaultRoute = routingRule;
                    }
                    else
                    {
                        validationResult.IsValid = false;
                        validationResult.Messages.Add("The list of routing rules supplied contains two default routes i.e. a rule with expression '{ }', please reduce to one");
                    }
                }

                if (!backendIds.Contains(routingRule.BackendId, StringComparer.InvariantCultureIgnoreCase))
                {
                    validationResult.IsValid = false;
                    validationResult.Messages.Add($"Routing Rule '{routingRule.Expression}' references a backend '{routingRule.BackendId}' that is not defined, please define backend or remove rule");
                }
            }

            if (defaultRoute == null)
            {
                validationResult.IsValid = false;
                validationResult.Messages.Add("The list of routing rules supplied does not have a default route i.e. a rule with expression '{ }'");
            }

            return validationResult;
        }
    }
}
