#nullable enable
using System;
using System.Linq;

namespace NetElevation.Core
{
    public class TileManager
    {
        private readonly ITileRepository _repository;
        private readonly Lazy<TileInfo[]> _tiles;

        public TileManager(ITileRepository repository)
        {
            _repository = repository;
            _tiles = new Lazy<TileInfo[]>(_repository.GetTiles);
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

        private short[] GetElevationMap(TileInfo tile) => _repository.GetElevationMap(tile);

        private TileInfo? GetTile(double latitude, double longitude) => GetTiles().FirstOrDefault(t => t.Contains(latitude, longitude));
    }
}
