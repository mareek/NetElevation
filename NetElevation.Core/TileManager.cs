using System.Linq;

namespace NetElevation.Core
{
    public class TileManager
    {
        private const int MaxCacheSize = 100_000_000;

        private readonly ElevationMapCache _cache;
        private readonly TileTreeNode _tileTreeRoot;

        public TileManager(ITileRepository repository, int maxCacheSize = MaxCacheSize)
        {
            _cache = new ElevationMapCache(repository, maxCacheSize);
            _tileTreeRoot = TreeBuilder.BuildTree(repository.GetTiles());
        }

        public short GetElevation(double latitude, double longitude)
        {
            var location = new Location { Latitude = latitude, Longitude = longitude };
            SetElevations(location);
            return location.Elevation ?? 0;
        }

        public void SetElevations(params Location[] locations)
        {
            var defaultTile = new TileInfo { North = 1000, West = -1000 };
            var coordinatesByTile = locations.GroupBy(l => _tileTreeRoot.GetTile(l.Latitude, l.Longitude) ?? defaultTile);
            foreach (var tileGroup in coordinatesByTile)
            {
                var tile = tileGroup.Key;
                if (tile == defaultTile)
                {
                    foreach (var location in tileGroup)
                    {
                        location.Elevation = 0;
                    }
                }
                else
                {
                    var elevationMap = _cache.GetValue(tile);
                    foreach (var location in tileGroup)
                    {
                        location.Elevation = tile.GetElevation(location.Latitude, location.Longitude, elevationMap);
                    }
                }
            }
        }
    }
}
