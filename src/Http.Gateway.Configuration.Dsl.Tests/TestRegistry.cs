using System;
using System.Collections.Generic;

namespace Http.Gateway.Configuration.Dsl.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    class TestRegistry : IRegistry
    {
        public string Id
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public void Configure(ConfigurationMap configurationObject)
        {
            throw new NotImplementedException();
        }

        public void Initialize(ConfigurationMap configurationObject)
        {
            throw new NotImplementedException();
        }

        public Uri Next()
        {
            throw new NotImplementedException();
        }
    }
}
