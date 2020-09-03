#nullable enable
using System;
using System.Linq;

namespace NetElevation.Core
{
    public class TileManager
    {
        private const int MaxCacheSize = 100_000_000;

        private readonly ElevationMapCache _cache;
        private readonly Lazy<TileInfo[]> _tiles;

        public TileManager(ITileRepository repository, int maxCacheSize = MaxCacheSize)
        {
            _cache = new ElevationMapCache(repository, maxCacheSize);
            _tiles = new Lazy<TileInfo[]>(repository.GetTiles);
        }

        private TileInfo[] GetTiles() => _tiles.Value;

        public short GetElevation(double latitude, double longitude)
        {
            var tile = GetTile(latitude, longitude);
            if (tile == null)
            {
                return 0;
            }

            return tile.GetElevation(latitude, longitude, GetElevationMap(tile));
        }

        private short[] GetElevationMap(TileInfo tile) => _cache.GetElevationMap(tile);

        private TileInfo? GetTile(double latitude, double longitude) => GetTiles().FirstOrDefault(t => t.Contains(latitude, longitude));
    }
}
