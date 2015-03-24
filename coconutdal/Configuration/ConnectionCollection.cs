using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections;

namespace CoconutDal.Configuration
{
    /// <summary>
    /// Represents a collection of Connection Configuration elements in a CoconutDalConfigurationSection
    /// </summary>
    public class ConnectionCollection : ConfigurationElementCollection, IList<ConnectionConfig>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionCollection"/> class.
        /// </summary>
        public ConnectionCollection()
        {

        }

        /// <summary>
        /// Gets the configuration element at the specified location
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ConnectionConfig this[int index]
        {
            get { return (ConnectionConfig)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        /// <summary>
        /// Adds a configuration element to the collection
        /// </summary>
        /// <param name="serviceConfig"></param>
        public void Add(ConnectionConfig serviceConfig)
        {
            BaseAdd(serviceConfig);
        }

        /// <summary>
        /// Removes all configuration elements from the collection
        /// </summary>
        public void Clear()
        {
            BaseClear();
        }

        /// <summary>
        /// Creates a new configuration element
        /// </summary>
        /// <returns></returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new ConnectionConfig();
        }

        /// <summary>
        /// Gets the property value that is used as a key for this configuration element
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ConnectionConfig)element).Name;
        }

        /// <summary>
        /// Removes a configuration element from the collection
        /// </summary>
        /// <param name="serviceConfig"></param>
        public void Remove(ConnectionConfig serviceConfig)
        {
            if (serviceConfig != null)
                BaseRemove(serviceConfig.Name);
        }
        /// <summary>
        /// Removes the configuration element at the specified location
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        /// <summary>
        /// Removes a configuration element from the collection
        /// </summary>
        /// <param name="name"></param>
        public void Remove(string name)
        {
            BaseRemove(name);
        }

        #region IList<ConnectionConfig> Members

        /// <summary>
        /// The index of the specified configuration element
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(ConnectionConfig item)
        {
            return BaseIndexOf(item);
        }

        /// <summary>
        /// Adds a configuration element to the collection
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, ConnectionConfig item)
        {
            BaseAdd(index, item);
        }

        #endregion

        #region ICollection<ConnectionConfig> Members

        /// <summary>
        /// Determines whether the collection contains the specified configuration element
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(ConnectionConfig item)
        {
            return BaseIndexOf(item) >= 0;
        }

        /// <summary>
        /// Copies items from the current collection into the specified array instance, starting at the specified index.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(ConnectionConfig[] array, int arrayIndex)
        {
            if (object.ReferenceEquals(array, null))
            {
                throw new ArgumentNullException(
                    "Null array reference",
                    "array"
                    );
            }

            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "Index is out of range",
                    "arrayIndex"
                    );
            }

            if (array.Rank > 1)
            {
                throw new ArgumentException(
                    "Array is multi-dimensional",
                    "array"
                    );
            }

            foreach (object o in this)
            {
                array.SetValue(o, arrayIndex);
                arrayIndex++;
            }
        }

        /// <summary>
        /// Indicates that the Collection is read only
        /// </summary>
        public new bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes a configuration element from the collection
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool ICollection<ConnectionConfig>.Remove(ConnectionConfig item)
        {
            int index = BaseIndexOf(item);
            if (index >= 0)
            {
                BaseRemoveAt(index);
                return true;
            }
            return false;
        }

        #endregion

        #region IEnumerable<ConnectionConfig> Members

        /// <summary>
        /// Gets an System.Collections.IEnumerator which is used to iterate through the
        /// ConfigurationElementCollection.
        /// </summary>
        /// <returns></returns>
        public new IEnumerator<ConnectionConfig> GetEnumerator()
        {
            return new DatabaseCollectionEnumerator(base.GetEnumerator());
        }

        #endregion

        private class DatabaseCollectionEnumerator : IEnumerator<ConnectionConfig>
        {
            private IEnumerator _enumerator;
            public DatabaseCollectionEnumerator(IEnumerator enumerator)
            {
                _enumerator = enumerator;
            }

            public ConnectionConfig Current
            {
                get { return (ConnectionConfig)_enumerator.Current; }
            }

            object IEnumerator.Current
            {
                get { return _enumerator.Current; }
            }

            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            public void Reset()
            {
                _enumerator.Reset();
            }

            public void Dispose()
            {
            }

        }

    }
}
