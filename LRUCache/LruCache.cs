using System.Collections.Generic;

namespace LruCache
{
    /// <summary>
    /// A least-recently-used cache stored like a dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the key to the cached item.</typeparam>
    /// <typeparam name="TValue">The type of the cached item.</typeparam>
    public class LruCache<TKey, TValue>
    {
        private readonly int _capacity;
        private readonly Dictionary<TKey, TValue> _dictionary;
        private readonly LinkedList<TKey> _cacheList;
        
        /// <summary>
        /// Gets capacity of the cache. 
        /// </summary>
        public int Capacity => _capacity;
        
        /// <summary>
        /// Get utilization of the cache storage.
        /// </summary>
        public float Utilization => _dictionary.Count / _capacity;

        /// <summary>
        /// Initializes a new instance of the <see cref="LruCache{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="capacity">Maximum number of elements to cache.</param>
        public LruCache(int capacity)
        {
            
            _capacity = capacity;
            _dictionary = new Dictionary<TKey, TValue>();
            _cacheList = new LinkedList<TKey>();
        }

        /// <summary>
        /// Adds the specified key and value to the cache.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        public void Put(TKey key, TValue value)
        {
            if (_dictionary.ContainsKey(key))
            {
                _dictionary[key] = value;
                _cacheList.Remove(key);
                _cacheList.AddFirst(key);
            }
            else
            {
                if (_dictionary.Count >= _capacity)
                {
                    _dictionary.Remove(_cacheList.Last.Value);
                    _cacheList.RemoveLast();
                }

                _dictionary.Add(key,value);

                _cacheList.AddFirst(key);
            }
        }
        
        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>Value associated with the specified key.</returns>
        /// <exception cref="KeyNotFoundException">Key does not exist in cache.</exception>
        public TValue Get(TKey key)
        {
            if(!_dictionary.TryGetValue(key, out TValue value))
                throw new KeyNotFoundException($"key: {key}");

            _cacheList.Remove(key);
            _cacheList.AddFirst(key);

            return value;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
        /// <returns>True if contains an element with the specified key; otherwise, false.</returns>
        public bool TryGet(TKey key, out TValue value)
        {
            if(!_dictionary.TryGetValue(key, out value))
                return false;

            _cacheList.Remove(key);
            _cacheList.AddFirst(key);
            
            return true;
        }

        /// <summary>
        /// Remove all elements from cache.
        /// </summary>
        public void Clear()
        {
            _dictionary.Clear();
            _cacheList.Clear();
        }
    }
}