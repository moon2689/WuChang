using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SakashoUISystem
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys = new List<TKey>();
        [SerializeField]
        private List<TValue> values = new List<TValue>();

        private IDictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        #region IDictionary implementation

        public void Add(TKey key, TValue value)
        {
            dictionary.Add(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            return dictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public TValue this[TKey index] {
            get { return dictionary[index]; }
            set { dictionary[index] = value; }
        }

        public ICollection<TKey> Keys {
            get { return dictionary.Keys; }
        }

        public ICollection<TValue> Values {
            get { return dictionary.Values; }
        }

        #endregion

        #region ICollection implementation

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            dictionary.Add(item);
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            dictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return dictionary.Remove(item);
        }

        public int Count {
            get { return dictionary.Count; }
        }

        public bool IsReadOnly {
            get { return dictionary.IsReadOnly; }
        }

        #endregion

        #region IEnumerable implementation

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        #endregion

        #region IEnumerable implementation

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        #endregion

        #region ISerializationCallbackReceiver implementation

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (var pair in dictionary) {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }
        public void OnAfterDeserialize()
        {
            dictionary = new Dictionary<TKey, TValue>();
            if (keys.Count != values.Count) {
                throw new System.Exception("The count of keys doesn't match that of values");
            }
            for (int i = 0; i < keys.Count; i++) {
                dictionary.Add(keys[i], values[i]);
            }
        }

        #endregion
    }



}