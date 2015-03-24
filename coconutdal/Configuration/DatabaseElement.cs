using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace CoconutDal.Configuration
{
    /// <summary>
    /// Represents the Database Configuration element of a CoconutDalConfigurationSection
    /// </summary>
    public class DatabaseElement : ConfigurationElement
    {
        /// <summary>
        /// A collection of connection strings
        /// </summary>
        [ConfigurationProperty("Connections", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(ConnectionCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public ConnectionCollection Connections
        {
            get
            {
                return (ConnectionCollection)base["Connections"];
            }
        }

        /// <summary>
        /// When specified, the value of this property indicates which single connection string should be used
        /// </summary>
        [ConfigurationProperty("UseName", IsRequired = false)]
        public string UseName
        {
            get
            {
                return (string)this["UseName"];
            }
            set
            {
                this["UseName"] = value;
            }
        }

        /// <summary>
        /// When specified, the value of this property indicates which group of connection strings should be used
        /// </summary>
        [ConfigurationProperty("UseSuffix", IsRequired = false)]
        public string UseSuffix
        {
            get
            {
                return (string)this["UseSuffix"];
            }
            set
            {
                this["UseSuffix"] = value;
            }
        }
        /// <summary>
        /// Initializes a new instance of the DatabaseElement class
        /// </summary>
        /// <param name="useName"></param>
        /// <param name="useGroup"></param>
        public DatabaseElement(string useName, string useGroup)
        {
            this.UseName = useName;
            this.UseSuffix = useGroup;
        }

        /// <summary>
        /// Initializes a new instance of the DatabaseElement class
        /// </summary>
        public DatabaseElement()
        {

        }
    }
}
