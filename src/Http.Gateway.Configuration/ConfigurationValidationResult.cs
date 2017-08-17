using System.Collections.Generic;

namespace Http.Gateway.Configuration
{
    public class ConfigurationValidationResult
    {
        public bool IsValid { get; internal set; }
        public List<string> Messages { get; internal set; }

        public ConfigurationValidationResult()
        {
            Messages = new List<string>();
        }
    }
}