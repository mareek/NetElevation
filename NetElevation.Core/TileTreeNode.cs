#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace NetElevation.Core
{
    public class TileTreeNode : BaseCoordinates
    {
        private readonly List<TileInfo> _tiles;
        private List<TileTreeNode> _children;

        public TileTreeNode(double north, double west, double latitudeSpan, double longitudeSpan)
            : base(north, west, latitudeSpan, longitudeSpan)
        {
            _children = new List<TileTreeNode>();
            _tiles = new List<TileInfo>();
        }

        public bool IsEmpty => IsLeaf && !_tiles.Any();
        
        public bool IsLeaf => !_children.Any();

        public IEnumerable<TileTreeNode> Children => _children.AsReadOnly();

        public void AddChildren(params TileTreeNode[] children) => _children.AddRange(children);

        public void AddTile(TileInfo tile) => _tiles.Add(tile);

        public void AddTiles(params TileInfo[] tiles) => _tiles.AddRange(tiles);

        public void RemoveEmptyChildren()
        {
            foreach (var child in _children)
            {
                child.RemoveEmptyChildren();
            }

            _children = _children.Where(c => !c.IsEmpty).ToList();
        }

        public TileInfo? GetTile(double latitude, double longitude)
        {
            if (IsEmpty || !Contains(latitude, longitude))
            {
                return null;
            }
            else if (_children.Any())
            {
                return _children.Select(c => c.GetTile(latitude, longitude))
                                .FirstOrDefault(t => t != null);
            }
            else
            {
                return _tiles.FirstOrDefault(t => t.Contains(latitude, longitude));
            }
        }
    }
}
