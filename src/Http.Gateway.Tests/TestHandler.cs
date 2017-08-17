using System;

namespace Http.Gateway.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class TestHandler : Module
    {
        public string AStringSetting { get; set; }

        public int AnIntSetting { get; set; }

        public override void Initialize(ConfigurationMap configurationObject)
        {
            if (configurationObject.ContainsKey("AStringSetting"))
            {
                AStringSetting = configurationObject["AStringSetting"] as string;
            }

            if (configurationObject.ContainsKey("AnIntSetting"))
            {
                AnIntSetting = Convert.ToInt32(configurationObject["AnIntSetting"]);
            }
        }

        public override void Configure(ConfigurationMap configurationObject)
        {
            
        }
    }
}
