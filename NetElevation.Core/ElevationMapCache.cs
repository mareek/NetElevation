using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace NetElevation.Core
{
    public class ElevationMapCache
    {
        private readonly ITileRepository _repository;
        private readonly ConcurrentDictionary<string, short[]> _cache;
        private readonly ConcurrentDictionary<string, DateTime> _lastTouched;
        private readonly int _maxCacheSize;
        private int _currentCacheSize;

        public ElevationMapCache(ITileRepository repository, int maxCacheSize)
        {
            _repository = repository;
            _maxCacheSize = maxCacheSize;
            _cache = new ConcurrentDictionary<string, short[]>();
            _lastTouched = new ConcurrentDictionary<string, DateTime>();
            _currentCacheSize = 0;
        }

        public short[] GetElevationMap(TileInfo tileInfo)
        {
            TouchTile(tileInfo);
            short[] elevationMap;
            if (!_cache.TryGetValue(tileInfo.FileName, out elevationMap))
            {
                //GetOrAdd is not atomic so we lock on tileInfo to prevent the map to be loaded from disk multiple times
                lock (tileInfo)
                {
                    elevationMap = _cache.GetOrAdd(tileInfo.FileName, _ => LoadElevationMap(tileInfo));
                }

                TrimCacheIfNeeded(_maxCacheSize);
            }

            return elevationMap;
        }

        private void TouchTile(TileInfo tileInfo) => _lastTouched[tileInfo.FileName] = DateTime.UtcNow;

        private short[] LoadElevationMap(TileInfo tileInfo)
        {
            var elevationMap = _repository.LoadElevationMap(tileInfo);
            IncreaseCurrentCacheSize(elevationMap);
            return elevationMap;
        }

        private int IncreaseCurrentCacheSize(short[] elevationMap) 
            => Interlocked.Add(ref _currentCacheSize, sizeof(short) * elevationMap.Length);

        private void DecreaseCurrentCacheSize(short[] elevationMap) 
            => Interlocked.Add(ref _currentCacheSize, -sizeof(short) * elevationMap.Length);

        private void TrimCacheIfNeeded(int maxCacheSize)
        {
            if (_currentCacheSize > maxCacheSize)
            {
                var targetCacheSize = 3 * maxCacheSize / 4;

                string[] tileNameByDate = _lastTouched.OrderBy(kvp => kvp.Value).Select(kvp => kvp.Key).ToArray();

                int i = 0;
                while (_currentCacheSize > targetCacheSize && i < tileNameByDate.Length)
                {
                    if (_cache.TryRemove(tileNameByDate[i], out var elevationMap))
                    {
                        _lastTouched.TryRemove(tileNameByDate[i], out var _);
                        DecreaseCurrentCacheSize(elevationMap);
                    }

                    i += 1;
                }
            }
        }
    }
}
