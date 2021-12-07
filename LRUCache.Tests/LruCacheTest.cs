using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using LruCache;
using NUnit.Framework;
using LRUCache;
namespace LRUCache.Tests
{
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
        public void Put_NullValue_StoreNull()
        {
            var cache = GetCache<string, string>(10);

            var cacheKey = "key";

            cache.Put(cacheKey, null);

            var val1 = cache.Get(cacheKey);
            cache.TryGet(cacheKey, out var val2);
            
            Assert.That(val1, Is.Null);
            Assert.That(val2, Is.Null);
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

        [Test]
        public void Remove_NullKey_ThrowsArgumentNullException()
        {
            var cache = GetCache<string, string>();
            
            Assert.That(()=> cache.Remove(null), Throws.Exception.TypeOf<ArgumentNullException>());
        }
        
        [Test]
        public void Remove_ContainingKey_ReturnTrue()
        {
            var cache = GetCache<string, int>();

            var cacheKey = "key";
            cache.Put(cacheKey, 1);

            bool result = cache.Remove(cacheKey);
            
            Assert.That(result, Is.True);
        }

        [Test]
        public void Remove_ContainingKey_KeyDeleted()
        {
            var cache = GetCache<string, int>();

            var cacheKey = "key";
            cache.Put(cacheKey, 1);
            cache.Remove(cacheKey);

            bool result = cache.TryGet(cacheKey, out _);
            
            Assert.That(result, Is.False);
        }
        
        [Test]
        public void Remove_MissingKey_ReturnFalse()
        {
            var cache = GetCache<string, int>();

            var cacheKey = "key";
            cache.Put(cacheKey, 1);

            bool result = cache.Remove("cacheKey");
            
            Assert.That(result, Is.False);
        }
        
        [Test]
        public void Remove_MissingKey_DoesntAffectAnotherKeys()
        {
            var cache = GetCache<string, string>(10);

            KickOutValues(cache);
            bool result = cache.Remove("cacheKey");
                
            Assert.That(result, Is.False);
            Assert.That(cache.Utilization, Is.EqualTo(1));
        }

        [Test]
        public void Remove_EmptyCache_ReturnFalse([Random(1,1000,1)]int size)
        {
            var cache = GetCache<string, int>(size);
            
            bool result = cache.Remove("cacheKey");
            
            Assert.That(result, Is.False);
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