namespace BaZic.StandaloneRuntime
{
    /// <summary> 
    /// Provides a thread-safe dictionary for use with data binding. 
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Count={Count}")]
    [System.Serializable]
    public class ObservableDictionary : System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object, object>>, System.Collections.Generic.IDictionary<object, object>, System.Collections.Specialized.INotifyCollectionChanged, System.ComponentModel.INotifyPropertyChanged, System.Runtime.Serialization.ISerializable
    {
        #region Fields & Constants

        [System.NonSerialized]
        private readonly System.Threading.SynchronizationContext _synchronizationContext = System.ComponentModel.AsyncOperationManager.SynchronizationContext;

        private readonly System.Collections.Concurrent.ConcurrentDictionary<object, object> _dictionary = new System.Collections.Concurrent.ConcurrentDictionary<object, object>();

        #endregion

        #region Properties

        public System.Collections.Generic.ICollection<object> Values
        {
            get { return _dictionary.Values; }
        }

        public object this[object key]
        {
            get { return _dictionary[key]; }
            set { UpdateWithNotification(key, value); }
        }

        public System.Collections.Generic.ICollection<object> Keys
        {
            get { return _dictionary.Keys; }
        }

        /// <inheritdoc/>
        public int Count
        {
            get { return _dictionary.Count; }
        }

        /// <inheritdoc/>
        public bool IsReadOnly
        {
            get { return ((System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object, object>>)_dictionary).IsReadOnly; }
        }

        #endregion

        #region Events

        /// <summary>Event raised when the collection changes.</summary> 
        public event System.Collections.Specialized.NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>Event raised when a property on the collection changes.</summary> 
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Methods

        /// <summary>
        /// Adds a new item to the dictionary and generates automatically its key.
        /// </summary>
        /// <param name="item">The item to add</param>
        public void Add(object item)
        {
            Add(new System.Collections.Generic.KeyValuePair<object, object>(GenerateNewKey(), item));
        }

        /// <inheritdoc/>
        public void Add(System.Collections.Generic.KeyValuePair<object, object> item)
        {
            TryAddWithNotification(item);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _dictionary.Clear();
            NotifyObserversOfChange();
        }

        /// <summary>
        /// Wheck whether the specified value exists in the values of the dictionary.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool ContainsValue(object item)
        {
            return Values.Contains(item);
        }

        /// <inheritdoc/>
        public bool Contains(System.Collections.Generic.KeyValuePair<object, object> item)
        {
            return ((System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object, object>>)_dictionary).Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(System.Collections.Generic.KeyValuePair<object, object>[] array, int arrayIndex)
        {
            ((System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object, object>>)_dictionary).CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public bool Remove(System.Collections.Generic.KeyValuePair<object, object> item)
        {
            return TryRemoveWithNotification(item.Key);
        }

        /// <inheritdoc/>
        System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object, object>> System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object, object>>.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        /// <inheritdoc/>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        /// <inheritdoc/>
        public void Add(object key, object value)
        {
            TryAddWithNotification(key, value);
        }

        /// <inheritdoc/>
        public bool ContainsKey(object key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <inheritdoc/>
        public bool Remove(object key)
        {
            return TryRemoveWithNotification(key);
        }

        /// <inheritdoc/>
        public bool TryGetValue(object key, out object value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        /// <inheritdoc/>
        [System.Security.SecurityCritical]
        public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            info.SetType(typeof(System.Collections.Generic.Dictionary<object, object>));
            var dictionary = new System.Collections.Generic.Dictionary<object, object>(_dictionary);
            dictionary.GetObjectData(info, context);
        }

        /// <summary>
        /// Gets the value that match the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Returns null if no key has been found.</returns>
        public object GetValueFromKey(object key)
        {
            object value = null;
            _dictionary.TryGetValue(key, out value);
            return value;
        }

        /// <summary>
        /// Gets the key that match the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Returns null if no value has been found.</returns>
        public object GetKeyFromValue(object value)
        {
            return System.Linq.Enumerable.FirstOrDefault(_dictionary, item => item.Value.Equals(value) || item.Value == value).Key;
        }

        /// <summary> 
        /// Notifies observers of CollectionChanged or PropertyChanged of an update to the dictionary. 
        /// </summary> 
        private void NotifyObserversOfChange()
        {
            var collectionHandler = CollectionChanged;
            var propertyHandler = PropertyChanged;
            if (collectionHandler != null || propertyHandler != null)
            {
                var action = new System.Action(() =>
                {
                    collectionHandler?.Invoke(this, new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));

                    if (propertyHandler != null)
                    {
                        propertyHandler(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Count)));
                        propertyHandler(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Keys)));
                        propertyHandler(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Values)));
                    }
                });

                if (System.Threading.SynchronizationContext.Current == _synchronizationContext)
                {
                    action();
                }
                else
                {
                    _synchronizationContext.Send(_ => action(), null);
                }
            }
        }

        /// <summary>Attempts to add an item to the dictionary, notifying observers of any changes.</summary> 
        /// <param name="item">The item to be added.</param> 
        /// <returns>Whether the add was successful.</returns> 
        private void TryAddWithNotification(System.Collections.Generic.KeyValuePair<object, object> item)
        {
            TryAddWithNotification(item.Key, item.Value);
        }

        /// <summary>Attempts to add an item to the dictionary, notifying observers of any changes.</summary> 
        /// <param name="key">The key of the item to be added.</param> 
        /// <param name="value">The value of the item to be added.</param> 
        /// <returns>Whether the add was successful.</returns> 
        private void TryAddWithNotification(object key, object value)
        {
            _dictionary.TryAdd(key, value);
            NotifyObserversOfChange();
        }

        /// <summary>Attempts to remove an item from the dictionary, notifying observers of any changes.</summary> 
        /// <param name="key">The key of the item to be removed.</param>
        /// <returns>Whether the removal was successful.</returns> 
        private bool TryRemoveWithNotification(object key)
        {
            object fake;
            bool result = _dictionary.TryRemove(key, out fake);
            if (result)
            {
                NotifyObserversOfChange();
            }
            return result;
        }

        /// <summary>Attempts to add or update an item in the dictionary, notifying observers of any changes.</summary> 
        /// <param name="key">The key of the item to be updated.</param> 
        /// <param name="value">The new value to set for the item.</param> 
        /// <returns>Whether the update was successful.</returns> 
        private void UpdateWithNotification(object key, object value)
        {
            _dictionary[key] = value;
            NotifyObserversOfChange();
        }

        /// <summary>
        /// Generates a new key that does not exist in the dictionary yet.
        /// </summary>
        /// <returns>Returns a <see cref="Int32"/> that does not exists in the list of keys.</returns>
        private int GenerateNewKey()
        {
            var i = Count;

            while (Keys.Contains(i))
            {
                i++;
            }

            return i;
        }

        #endregion
    }
}
