using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway.Configuration.Dsl
{
    public class RegistryParser
    {
        public ParserResult<List<RegistryDefinition>> ParseRegistries(string parse)
        {
            parse = parse.TrimStart('\n', '\r', ' ');
            var grammar = new RegistryGrammar();

            var parser = new Irony.Parsing.Parser(grammar);

            var parseResult = parser.Parse(parse);

            var result = new ParserResult<List<RegistryDefinition>>();

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
                result.Result = Builder.BuildRegistryDefintions(parseResult.Root.ChildNodes[0].ChildNodes.ToArray());
            }
            else
            {
                result.Result = new List<RegistryDefinition>();
            }

            return result;
        }
    }
}
