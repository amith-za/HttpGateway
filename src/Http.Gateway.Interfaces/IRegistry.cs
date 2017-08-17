using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway
{
    public interface IRegistry
    {
        /// <summary>
        /// Initialize the Registry instance using the provided configuration object
        /// </summary>
        /// <param name="configurationObject">Dynamic configuration object</param>
        void Initialize(ConfigurationMap configurationObject);

        /// <summary>
        /// Configure the Registry instance for use
        /// </summary>
        /// <param name="configurationObject">Dynamic configuration object</param>
        void Configure(ConfigurationMap configurationObject);

        /// <summary>
        /// Provides the Uri of the next available service
        /// </summary>
        /// <returns>The Uri of the next available service, null is there are no instances available</returns>
        Uri Next();
    }
}
