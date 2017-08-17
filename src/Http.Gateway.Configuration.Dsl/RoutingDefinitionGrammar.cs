using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway.Configuration.Dsl
{
    class RoutingDefinitionGrammar : Grammar
    {
        private List<string> _allowedRegistries = null;
        private List<string> _definedBackends = null;
        private List<string> _allowedModules = null;

        public RoutingDefinitionGrammar(List<string> allowedRegistries, List<string> allowedModules)
            : base(true)
        {
            _allowedRegistries = allowedRegistries;
            _definedBackends = new List<string>();
            _allowedModules = allowedModules;

            NewLineTerminal NewLine = new NewLineTerminal("LF");

            var root = new NonTerminal("root");

            // Configuration Object
            var propertyString = new StringLiteral("string", "\"");
            var propertyNumber = new NumberLiteral("number");
            var array = new NonTerminal("Array");
            var arrayBr = new NonTerminal("ArrayBr");

            var configurationBr = new NonTerminal("configurationBr");
            var configObject = new NonTerminal("configObject");
            var configProperty = new NonTerminal("configProperty");
            var propertyId = new IdentifierTerminal("propertyId", "_");
            var propertyValue = new NonTerminal("propertyValue");

            configurationBr.Rule = "{" + configObject + "}";
            configObject.Rule = MakeStarRule(configObject, ToTerm(","), configProperty);
            configProperty.Rule = propertyId + "=" + propertyValue;
            propertyValue.Rule = propertyString | propertyNumber | configObject | arrayBr | "True" | "true" | "False" | "false" | "null";
            arrayBr.Rule = "[" + array + "]";
            array.Rule = MakeStarRule(array, ToTerm(","), propertyValue);

            MarkPunctuation("{", "}", "[", "]", ":", ",");
            this.MarkTransient(propertyValue, arrayBr, configurationBr);

            var identifier = new IdentifierTerminal("identifier", "._");

            var backendList = new NonTerminal("backendList");
            var backend = new NonTerminal("backend");
            var backendId = new StringLiteral("backendId", "\"");
            backendId.ValidateToken += AppendToDefinedBackends;
            var registryId = new IdentifierTerminal("registryId", "._");
            registryId.ValidateToken += ValidateRegistryId;
            backendList.Rule = MakePlusRule(backendList, null, backend);
            backend.Rule = "backend" + backendId + "from" + registryId + configurationBr;

            var routes = new NonTerminal("match");
            var routeList = new NonTerminal("routeList");
            var route = new NonTerminal("route");
            var routeVerb = new IdentifierTerminal("routeVerb");
            var routeTemplate = new StringLiteral("routeTemplate", "\"");
            routeList.Rule = MakePlusRule(routeList, null, route);
            route.Rule = routeVerb + routeTemplate;

            var routeModules = new NonTerminal("routeModules");
            var routeModule = new NonTerminal("routeModule");
            var routeModuleId = new IdentifierTerminal("routeBackEnd");
            //routeModuleId.ValidateToken += ValidateRouteModuleId;
            routeModule.Rule = routeModuleId + configurationBr;
            routeModules.Rule = MakePlusRule(routeModules, null, routeModule);

            var routePredicates = new NonTerminal("routePredicates");
            var routePredicate = new NonTerminal("routePredicate");

            var predicate = new WikiBlockTerminal("predicateCodeBlock", WikiBlockType.CodeBlock, "{", "}", "text");
            predicate.ValidateToken += ValidateRuleCode;
            var routeBackend = new StringLiteral("routeBackend", "\"");
            routeBackend.ValidateToken += ValidateRouteBackend;
            routePredicate.Rule = predicate + "use" + routeBackend;

            routePredicates.Rule = MakePlusRule(routePredicates, null, routePredicate);

            var modules = new NonTerminal("modules");
            modules.Rule = "modules" + routeModules;

            var optionalRouteModules = new NonTerminal("optionalRouteModules", Empty | modules);

            routes.Rule = "match" + routeList + optionalRouteModules + "when" + routePredicates;

            this.Root = root;

            //var optionalmoduleList = new NonTerminal("optionalModuleList", Empty | moduleList);
            //var optionalRegistryList = new NonTerminal("optionalRegistryList", Empty | registryList);
            root.Rule = //optionalmoduleList + optionalRegistryList + 
                backendList + routes;
        }

        private void ValidateRouteModuleId(object sender, ValidateTokenEventArgs e)
        {
            if (!_allowedModules.Contains(e.Token.ValueString))
            {
                e.SetError($"No module identified by '{e.Token.ValueString}'. Please use a valid module id.");
            }
        }

        private void ValidateRouteBackend(object sender, ValidateTokenEventArgs e)
        {
            if (!_definedBackends.Contains(e.Token.ValueString))
            {
                var builder = new StringBuilder();
                builder.Append("| ");
                foreach (var item in _definedBackends)
                {
                    builder.Append(item);
                    builder.Append(" | ");
                }

                e.SetError($"The backend '{e.Token.ValueString}' is not defined. Options available are {builder.ToString()}");
            }
        }

        private void AppendToDefinedBackends(object sender, ValidateTokenEventArgs e)
        {
            if (_definedBackends.Contains(e.Token.ValueString))
            {
                e.SetError($"Backend identfier '{e.Token.ValueString}' has already been used. Please choose another name for this backend.");
            }
            else
            {
                _definedBackends.Add(e.Token.ValueString);
            }
        }

        private void ValidateRuleCode(object sender, ValidateTokenEventArgs e)
        {
            var rule = new RoutingRule(e.Token.ValueString, "none");

            if (!rule.IsValid)
            {
                e.SetError($"could not compile rule : {rule.ExpressionError}");
            }
        }

        private void ValidateRegistryId(object sender, ValidateTokenEventArgs e)
        {
            if (!_allowedRegistries.Contains(e.Token.ValueString))
            {
                e.SetError($"No registry identified by '{e.Token.ValueString}'. Valid options are {AllowedRegistyOptions()}");
            }
        }

        private string _options = null;

        private string AllowedRegistyOptions()
        {
            if (_options == null)
            {
                var builder = new StringBuilder();
                builder.Append('|');

                foreach (var item in _allowedRegistries)
                {
                    builder.Append(item);
                    builder.Append('|');
                }

                _options = builder.ToString();
            }

            return _options;
        }
    }
}
