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
            var tile = _tileTreeRoot.GetTile(latitude, longitude);
            if (tile == null)
            {
                return 0;
            }

            return tile.GetElevation(latitude, longitude, _cache.GetElevationMap(tile));
        }
    }
}
