using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using LruCache;
using NUnit.Framework;
using LRUCache;
namespace LRUCache.Tests
{
    
    /*
        +    If you check for key in an empty lru, it returns null.
        +    If you add a key, then check it right away, it is returned.
        +   If you add a key, then check for a different key, it returns null.
        -   If you add a key, then delete that key, then check for the key, it returns null.
        +   If you add a key, then add enough other keys to the point your first key is removed, then check for that key, it returns null.
        -   If you add something without a key, it throws.
        -   If you delete a key from an empty lru, throw (or fail silently, if that’s what you prefer).
        -   If you add a key without a value, it throws (unless you want that behavior to match a delete, in which case, check that it’s null).
     */
    [TestFixture]
    public class LruCacheTest
    {
        [Test]
        public void Ctor_NegativeSizeValue_ThrowsException([Random(Int32.MinValue,0,1)] int size)
        {
            Assert.That(()=> GetCache<string,int>(size), Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Ctor_PositiveSizeValue_InitPropertyCapacity([Random(1,10000,1)] int size)
        {
            var cache = GetCache<string, string>(size);
            
            Assert.That(cache.Capacity, Is.EqualTo(size));
        }

        [Test] 
        public void Get_NullKey_ThrowsArgumentNullException()
        {
            var cache = GetCache<string, string>();
            
            Assert.That(()=> cache.Get(null), Throws.Exception.TypeOf<ArgumentNullException>());
        }
        
        [Test] 
        public void Get_MissingKey_ThrowsKeyNotFoundException()
        {
            var cache = GetCache<string, string>();
            
            Assert.That(()=> cache.Get("1"), Throws.Exception.TypeOf<KeyNotFoundException>());
        }

        [Test] 
        public void TryGet_NullKey_ThrowsArgumentNullException()
        {
            var cache = GetCache<string, string>();
            
            Assert.That(()=> cache.TryGet(null, out _), Throws.Exception.TypeOf<ArgumentNullException>());
        }
        
        [Test] 
        public void TryGet_MissingKey_ReturnFalse()
        {
            var cache = GetCache<string, int>();

            int value;
            var result = cache.TryGet("1", out value);
            
            Assert.That(result, Is.False);
            Assert.That(value, Is.EqualTo(default(int)));
        }

        [Test]
        public void Get_ContainingKey_ReturnValue([Random(Int32.MinValue,Int32.MaxValue, 1)]int value)
        {
            var cache = GetCache<string, int>();

            var cacheKey = "key";
            cache.Put(cacheKey, value);

            int cachedValue = cache.Get(cacheKey);
            
            Assert.That(cachedValue, Is.EqualTo(value));
        }
        
        [Test]
        public void TryGet_ContainingKey_ReturnTrueOutParamGetCachedValue([Random(Int32.MinValue,Int32.MaxValue, 1)]int value)
        {
            var cache = GetCache<string, int>();

            var cacheKey = "key";
            cache.Put(cacheKey, value);

            bool result = cache.TryGet(cacheKey, out var cachedValue);
            
            Assert.That(result, Is.True);
            Assert.That(cachedValue, Is.EqualTo(value));
        }

        [Test] 
        public void Get_NotEmptyCacheMissingKey_ThrowsKeyNotFoundException()
        {
            var cache = GetCache<string, string>();
            cache.Put("1", "");
            
            Assert.That(()=> cache.Get("2"), Throws.Exception.TypeOf<KeyNotFoundException>());
        }

        [Test] 
        public void TryGet_NotEmptyCacheMissingKey_ReturnFalse()
        {
            var cache = GetCache<string, string>();
            cache.Put("1", "");
            
            var result = cache.TryGet("2", out var value);
            
            Assert.That(result, Is.False);
            Assert.That(value, Is.EqualTo(default(string)));
        }
        
        [Test] 
        public void Get_KeyPushedThenKickedOutByLruSize_ThrowsKeyNotFoundException()
        {
            var cache = GetCache<string, string>();
            var cacheKey = "xxx";
            cache.Put(cacheKey, "value");
            cache = KickOutValues(cache);
            
            Assert.That(()=> cache.Get(cacheKey), Throws.Exception.TypeOf<KeyNotFoundException>());
        }
        
        [Test] 
        public void TryGet_KeyPushedThenKickedOutByLruSize_ReturnFalse()
        {
            var cache = GetCache<string, string>();
            var cacheKey = "xxx";
            cache.Put(cacheKey, "value");
            cache = KickOutValues(cache);
            
            var result = cache.TryGet(cacheKey, out var value);
            
            Assert.That(result, Is.False);
            Assert.That(value, Is.EqualTo(default(string)));
        }
        
        private LruCache<TKey, TValue> GetCache<TKey, TValue>(int size = 10)
        {
            return new LruCache<TKey, TValue>(size);
        }
        
        private LruCache<string, string> KickOutValues(LruCache<string, string> cache)
        {
            for (int i = 0; i < cache.Capacity; i++)
            {
                var s = i.ToString();
                cache.Put(s,s);
            }
            
            return cache;
        }
        
    }
}