using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Parsing;

namespace Http.Gateway.Configuration.Dsl
{
    public class ModuleParser
    {
        private List<string> _usedIdentifiers;

        public ParserResult<List<ModuleDefinition>> ParseModules(string parse)
        {
            parse = parse.TrimStart('\n', '\r', ' ');
            var grammar = new ModuleGrammar();

            var parser = new Irony.Parsing.Parser(grammar);

            var parseResult = parser.Parse(parse);

            var result = new ParserResult<List<ModuleDefinition>>();

            foreach (var message in parseResult.ParserMessages)
            {
                result.ParserMessages.Add(
                    new ParseMessage
                    {
                        Level = (MessageLevel)message.Level,
                        Location = new SourceLocation
                        {
                            Column = message.Location.Column,
                            Line = message.Location.Line + 1,
                            Position = message.Location.Position
                        },
                        Message = message.Message
                    });
            }

            if (!result.HasErrors)
            {
                result.Result = Builder.BuildModuleDefintions(parseResult.Root.ChildNodes[0].ChildNodes.ToArray());
            }
            else
            {
                result.Result = new List<ModuleDefinition>();
            }

            foreach (var item in result.Result)
            {
                if (!item.IsValid())
                {
                    result.ParserMessages.Add(
                        new ParseMessage
                        {
                            Level = MessageLevel.Error,
                            Location = new SourceLocation { Column = 1, Line = 1, Position = 1 },
                            Message = $"Module '{item.Id}' configuration failed : {item.ConfigurationErrorMessage}"
                        });
                }
            }

            return result;
        }
    }
}
