using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway.Configuration.Dsl
{
    internal class ModuleGrammar : Grammar
    {
        private List<string> _usedIdentifiers;

        internal ModuleGrammar()
        {
            _usedIdentifiers = new List<string>();

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
            identifier.ValidateToken += ValidateIdentifier;

            var moduleName = new IdentifierTerminal("identifier", "._");
            moduleName.ValidateToken += ValidateModuleName;

            // Module
            var module = new NonTerminal("module");
            var moduleList = new NonTerminal("moduleList");
            moduleList.Rule = MakePlusRule(moduleList, null, module);
            module.Rule = "module" + identifier + "is" + moduleName + configurationBr;

            this.Root = root;
            root.Rule = moduleList;
        }

        private void ValidateIdentifier(object sender, ValidateTokenEventArgs e)
        {
            if (_usedIdentifiers.Contains(e.Token.ValueString, StringComparer.InvariantCultureIgnoreCase))
            {
                e.SetError($"The module identifier '{e.Token.ValueString}' has already been used. Please choose another identifer for this module instance.");
            }
            else
            {
                _usedIdentifiers.Add(e.Token.ValueString);
            }
        }

        private void ValidateModuleName(object sender, ValidateTokenEventArgs e)
        {
            var type = Utility.ScanForType(e.Token.ValueString);

            if (type == null)
            {
                e.SetError($"Module type '{e.Token.ValueString}' could not be resolved. Does this script require a newer version of the Http Gateway?");
            }
        }
    }
}
