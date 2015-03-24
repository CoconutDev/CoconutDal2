using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace CoconutDal.Configuration
{
    /// <summary>
    /// Represents the CoconutDalConfigurationSection part of a configuration file.
    /// </summary>
    public class CoconutDalConfigurationSection : ConfigurationSection
    {
        /// <summary>
        /// Database element of the CoconutDalConfigurationSection
        /// </summary>
        [ConfigurationProperty("Database", IsRequired = true)]
        public DatabaseElement Database
        {
            get { return (DatabaseElement)this["Database"]; }
            set { this["Database"] = value; }
        }

        /// <summary>
        /// Searchs CoconutDalConfigurationSection and finds the connection string that is currently active.
        /// </summary>
        /// <returns></returns>
        public static ConnectionConfig GetEnvironmentConfig()
        {
            return GetEnvironmentConfig(string.Empty);
        }

        /// <summary>
        /// Searchs CoconutDalConfigurationSection and finds the connection string associated with the supplied name.
        /// </summary>
        /// <param name="name">Name of the connection string</param>
        /// <returns></returns>
        public static ConnectionConfig GetEnvironmentConfig(string name)
        {

            string configTypeName = typeof(CoconutDalConfigurationSection).Name;

            CoconutDalConfigurationSection databaseConfigSection =
                ConfigurationManager.GetSection("CoconutDalConfigurationSection") as CoconutDalConfigurationSection;

            if (databaseConfigSection == null || databaseConfigSection.Database ==null)
                throw new ArgumentException(configTypeName + " missing or does not contain a database element.");

            DatabaseElement databaseElement = databaseConfigSection.Database;

            if (databaseElement.Connections.Count < 1)
                throw new ArgumentException(configTypeName + " does not contain any connections.");
            

            ConnectionConfig connectionConfig = null;
            string key = string.IsNullOrEmpty(name) ? databaseElement.UseName : name;
            string group = databaseElement.UseSuffix;

            if (!string.IsNullOrEmpty(group))
                key += group;

            connectionConfig = databaseElement.Connections.Where(x => x.Name == key).FirstOrDefault();

            return connectionConfig ?? databaseElement.Connections[0];
        }

    }
}
