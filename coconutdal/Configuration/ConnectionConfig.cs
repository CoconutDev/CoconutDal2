using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace CoconutDal.Configuration
{
    /// <summary>
    /// Represents the Connection Configuration element of a CoconutDalConfigurationSection
    /// </summary>
    public class ConnectionConfig : ConfigurationElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionConfig"/> class.
        /// </summary>
        public ConnectionConfig() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionConfig"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="connectionString">The connection string.</param>
        public ConnectionConfig(string name, string connectionString)
        {
            Name = name;
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Gets or sets the name for this configuration
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [ConfigurationProperty("Name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this["Name"]; }
            set { this["Name"] = value; }
        }

        /// <summary>
        /// Gets or sets the connection string. This could be a database connection string or a service url.
        /// </summary>
        /// <value>
        /// The connection string/url.
        /// </value>
        [ConfigurationProperty("ConnectionString", IsRequired = true, IsKey = false)]
        public string ConnectionString
        {
            get { return (string)this["ConnectionString"]; }
            set { this["ConnectionString"] = value; }
        }
    }
}
