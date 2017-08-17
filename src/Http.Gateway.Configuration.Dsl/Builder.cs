using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway.Configuration.Dsl
{
    static class Builder
    {
        internal static List<ModuleDefinition> BuildModuleDefintions(params ParseTreeNode[] moduleNode)
        {
            var modules = new List<ModuleDefinition>();
            foreach (var node in moduleNode)
            {
                var moduleIdentifier = node.ChildNodes[1].Token.Text;
                var moduleType = node.ChildNodes[3].Token.Text;
                var map = BuildConfigurationMap(node.ChildNodes[4]);

                modules.Add(new ModuleDefinition(moduleIdentifier, moduleType, map));
            }

            return modules;
        }

        internal static List<RegistryDefinition> BuildRegistryDefintions(params ParseTreeNode[] registryNode)
        {
            var modules = new List<RegistryDefinition>();
            foreach (var node in registryNode)
            {
                var registryIdentifier = node.ChildNodes[1].Token.Text;
                var registryType = node.ChildNodes[3].Token.Text;
                var map = BuildConfigurationMap(node.ChildNodes[4]);

                modules.Add(new RegistryDefinition(registryIdentifier, registryType, map));
            }

            return modules;
        }

        internal static RoutingDefinition BuildRoutingDefinition(ParseTreeNode root)
        {
            var backends = BuildBackendDefinition(root.ChildNodes[0]);
            var routeInfo = BuildRouteInfo(root.ChildNodes[1]);
            var rules = BuildRoutingRules(root.ChildNodes[1]);

            var modules = new List<RouteModule>();

            var optionalModules = root.ChildNodes[1]?.ChildNodes[2];
            if (optionalModules.ChildNodes.Count > 0)
            {
                foreach (var module in optionalModules.ChildNodes[0]?.ChildNodes[1]?.ChildNodes)
                {
                    modules.Add(new RouteModule() { Id = module.ChildNodes[0].Token.Text, Configuration = BuildConfigurationMap(module.ChildNodes[1]) });
                }
            }

            var routingDefinition = new RoutingDefinition()
            {
                Backends = backends,
                Routes = routeInfo,
                RoutingRules = rules,
                RouteModules = modules
            };

            return routingDefinition;
        }

        private static List<RoutingRule> BuildRoutingRules(ParseTreeNode parseTreeNode)
        {
            var rules = new List<RoutingRule>();

            foreach (var item in parseTreeNode.ChildNodes[4].ChildNodes)
            {
                var expression = item.ChildNodes[0].Token.ValueString.TrimStart('{').TrimEnd('}');
                var backend = item.ChildNodes[2].Token.ValueString;

                rules.Add(new RoutingRule(expression, backend));
            }

            return rules;
        }

        internal static List<BackendDefinition> BuildBackendDefinition(ParseTreeNode root)
        {
            var backends = new List<BackendDefinition>();
            foreach (var node in root.ChildNodes)
            {
                var identifier = (string)node.ChildNodes[1].Token.Value;
                var registryId = (string)node.ChildNodes[3].Token.Value;
                var configMap = BuildConfigurationMap(node.ChildNodes[4]);

                backends.Add(new BackendDefinition { Id = identifier, RegistryId = registryId, RegistryConfiguration = configMap });
            }

            return backends;
        }

        internal static List<RouteInfo> BuildRouteInfo(ParseTreeNode root)
        {
            var routes = new List<RouteInfo>();

            foreach (var item in root.ChildNodes[1].ChildNodes)
            {
                var verb = item.ChildNodes[0].Token.ValueString;
                var routeTemplate = item.ChildNodes[1].Token.ValueString;

                routes.Add(new RouteInfo(verb, routeTemplate));
            }

            return routes;
        }

        internal static ConfigurationMap BuildConfigurationMap(ParseTreeNode configurationNode)
        {
            var map = new ConfigurationMap();
            foreach (var value in configurationNode.ChildNodes)
            {
                if (string.Equals(value.ChildNodes[2].Term?.Name, "Array"))
                {
                    var values = new List<string>();
                    foreach (var item in value.ChildNodes[2].ChildNodes)
                    {
                        values.Add(item.Token.ValueString);
                    }

                    map.Add(value.ChildNodes[0].Token.ValueString, values.ToArray());
                }
                else
                {
                    map.Add(value.ChildNodes[0].Token.ValueString, value.ChildNodes[2].Token.ValueString);
                }
            }

            return map;
        }
    }
}
