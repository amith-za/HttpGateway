using System.Collections.Generic;

namespace Http.Gateway.Configuration.Dsl
{
    public class ParserResult<T>
    {
        public T Result { get; set; }

        public bool HasErrors
        {
            get
            {
                foreach (var item in ParserMessages)
                {
                    if (item.Level == MessageLevel.Error)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public List<ParseMessage> ParserMessages { get; set; }

        public ParserResult()
        {
            ParserMessages = new List<ParseMessage>();
        }
    }
}