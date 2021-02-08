using System;
using System.Collections.Concurrent;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace TestShuffler
{
    public sealed class ExtendedCache<TKey, TValue> : IDisposable
    {
        private readonly ConcurrentDictionary<string, TKey> _computedKeyToKeyMapping;
        private readonly MemoryCache _valuesCache;
        private readonly CacheItemPolicy _valuesCacheItemPolicy;

        public ExtendedCache(TimeSpan expiryTime, Action<TValue> onExpired)
        {
            Ensure.NotNull(nameof(onExpired), onExpired);

            _valuesCacheItemPolicy =
                new CacheItemPolicy
                {
                    RemovedCallback = _ =>
                    {
                        _computedKeyToKeyMapping.TryRemove(_.CacheItem.Key, out var _);
                        onExpired((TValue)_.CacheItem.Value);
                    },
                    SlidingExpiration = expiryTime
                };

            _valuesCache = new MemoryCache(Guid.NewGuid().ToString());
            _computedKeyToKeyMapping = new ConcurrentDictionary<string, TKey>();
        }

        public void Dispose()
        {
            _valuesCache?.Dispose();
        }

        public void Set(TKey key, TValue value)
        {
            _valuesCache.Set(ComputeKey(key), value, _valuesCacheItemPolicy);
        }

        public TValue Get(TKey key) =>
            (TValue)_valuesCache.Get(ComputeKey(key));

        public TValue GetOrAdd(TKey key, TValue value)
        {
            var computedKey = ComputeKey(key);
            _computedKeyToKeyMapping[computedKey] = key;
            return (TValue)_valuesCache.AddOrGetExisting(computedKey, value, _valuesCacheItemPolicy);
        }

        public bool Contains(TKey key) =>
            _valuesCache.Contains(ComputeKey(key));

        private string ComputeKey(TKey key) =>
            key.GetHashCode().ToString();
    }
}