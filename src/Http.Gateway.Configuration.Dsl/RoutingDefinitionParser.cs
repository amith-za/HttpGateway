using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway.Configuration.Dsl
{
    public class RoutingDefinitionParser
    {
        public List<string> AllowedRegistries { get; private set; }

        public List<string> AllowedModules { get; private set; }

        public RoutingDefinitionParser(List<string> allowedRegistries, List<string> allowedModules)
        {
            AllowedRegistries = allowedRegistries ?? new List<string>();
            AllowedModules = allowedModules ?? new List<string>();
        }

        public RoutingDefinitionParser()
            : this(new List<string>(), new List<string>())
        {
        }

        public ParserResult<RoutingDefinition> ParseRoutingDefinition(string parse)
        {
            parse = parse.TrimStart('\n', '\r', ' ');
            var grammar = new RoutingDefinitionGrammar(AllowedRegistries, AllowedModules);

            var parser = new Irony.Parsing.Parser(grammar);

            var parseResult = parser.Parse(parse);

            var result = new ParserResult<RoutingDefinition>();

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
                result.Result = Builder.BuildRoutingDefinition(parseResult.Root);
            }

            return result;
        }
    }
}
