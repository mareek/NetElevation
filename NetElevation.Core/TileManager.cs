using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NetElevation.Core
{
    public class TileManager
    {
        private readonly TileRepository _repository;
        private readonly Lazy<TileInfo[]> _tiles;

        public TileManager(DirectoryInfo directory)
        {
            _repository = new TileRepository(directory);
            _tiles = new Lazy<TileInfo[]>(_repository.GetTiles);
        }

        private TileInfo[] GetTiles() => _tiles.Value;

        public short GetElevation(double latitude, double longitude)
        {
            var tile = GetTiles().FirstOrDefault(t => t.Contains(latitude, longitude));
            if (tile == null)
            {
                return 0;
            }

            var elevationMap = _repository.GetElevationMap(tile);
            return tile.GetElevation(latitude, longitude, elevationMap);
        }
    }
}
