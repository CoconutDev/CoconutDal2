using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoconutDal.Configuration
{
    /// <summary>
    /// Describes how CoconutDal should use configuration files
    /// </summary>
    public enum ConfigurationBehaviour
    {
        /// <summary>
        /// Ignore App/Web config files. A connection string must be supplied to the Dal's constructor.
        /// </summary>
        DoNotUseAppConfig
    }
}
