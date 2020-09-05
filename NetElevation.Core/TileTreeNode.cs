#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace NetElevation.Core
{
    public class TileTreeNode
    {
        private readonly List<TileInfo> _tiles;
        private List<TileTreeNode> _children;

        public TileTreeNode(double north, double west, double latitudeSpan, double longitudeSpan)
        {
            North = north;
            West = west;
            LatitudeSpan = latitudeSpan;
            LongitudeSpan = longitudeSpan;
            _children = new List<TileTreeNode>();
            _tiles = new List<TileInfo>();
        }

        public double North { get; }
        public double West { get; }

        public double LatitudeSpan { get; }
        public double LongitudeSpan { get; }

        public double South => North - LatitudeSpan;
        public double East => West + LongitudeSpan;

        public TileTreeNode? Parent { get; set; }
        public IReadOnlyCollection<TileTreeNode> Children => _children.AsReadOnly();

        private bool IsEmpty => !_children.Any() && !_tiles.Any();

        public bool Contains(double latitude, double longitude)
            => North >= latitude && latitude > South
                && East > longitude && longitude >= West;

        public void AddChildren(params TileTreeNode[] children)
        {
            _children.AddRange(children);
            foreach (var child in children)
            {
                child.Parent = this;
            }
        }

        public void AddTiles(params TileInfo[] tiles) => _tiles.AddRange(tiles);

        public void RemoveEmptyChildren() => _children = _children.Where(c => !c.IsEmpty).ToList();

        public TileInfo? GetTile(double latitude, double longitude)
        {
            if (IsEmpty || !Contains(latitude, longitude))
            {
                return null;
            }
            else if (_children.Any())
            {
                var child = _children.FirstOrDefault(c => c.Contains(latitude, longitude));
                return child?.GetTile(latitude, longitude);
            }
            else
            {
                return _tiles.First(t => t.Contains(latitude, longitude));
            }
        }
    }
}
