using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace NetElevation.Core
{
    public abstract class BaseCache<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _cache;
        private readonly ConcurrentDictionary<TKey, DateTime> _lastTouched;
        private readonly int _maxCacheSize;
        private int _currentCacheSize;

        protected BaseCache(int maxCacheSize)
        {
            _maxCacheSize = maxCacheSize;
            _cache = new ConcurrentDictionary<TKey, Lazy<TValue>>();
            _lastTouched = new ConcurrentDictionary<TKey, DateTime>();
            _currentCacheSize = 0;
        }

        protected abstract TValue LoadValue(TKey key);
        protected abstract int GetSize(TValue value);

        public TValue GetValue(TKey TKey)
        {
            TouchEntry(TKey);

            //GetOrAdd is not atomic in the sense that valueFactory can be call multiple
            //time if GetOrAdd is called concurently with the same key.
            //We circonvert this problem by using a Lazy<TValue> type instead of a TValue
            var entry = _cache.GetOrAdd(TKey, key => new Lazy<TValue>(() => LoadEntry(key)));
            var value = entry.Value;
            
            TrimCacheIfNeeded();

            return value;
        }

        private void TouchEntry(TKey TKey) => _lastTouched[TKey] = DateTime.UtcNow;

        private TValue LoadEntry(TKey TKey)
        {
            var entry = LoadValue(TKey);
            IncreaseCurrentCacheSize(entry);
            return entry;
        }

        private int IncreaseCurrentCacheSize(TValue entry)
            => Interlocked.Add(ref _currentCacheSize, GetSize(entry));

        private void DecreaseCurrentCacheSize(TValue entry)
            => Interlocked.Add(ref _currentCacheSize, -GetSize(entry));

        private void TrimCacheIfNeeded()
        {
            if (_currentCacheSize > _maxCacheSize)
            {
                /* We set a target size lower than the max size so that we don't run this 
                   method for each allocation once the cache is full */
                var targetCacheSize = 3 * _maxCacheSize / 4;

                TKey[] entryKeysByDate = _lastTouched.OrderBy(kvp => kvp.Value)
                                                     .Select(kvp => kvp.Key)
                                                     .ToArray();

                int i = 0;
                while (i < entryKeysByDate.Length)
                {
                    lock (_cache)
                    {
                        if (_currentCacheSize <= targetCacheSize)
                        {
                            break;
                        }

                        if (_cache.TryRemove(entryKeysByDate[i], out var entry))
                        {
                            _lastTouched.TryRemove(entryKeysByDate[i], out var _);
                            DecreaseCurrentCacheSize(entry.Value);
                        }
                    }

                    i += 1;
                }
            }
        }
    }
}
