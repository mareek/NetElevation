using System.Collections.Generic;
using System.Linq;

namespace NetElevation.Core
{
    public static class TreeBuilder
    {
        public static TileTreeNode BuildTree(TileInfo[] tiles)
        {
            var root = new TileTreeNode(90, -180, 180, 360);

            //first level: 90°*90°
            var firstLevel = CreateChildren(root, 2, 4).ToArray();
            //second level: 30°*30°
            var secondLevel = firstLevel.SelectMany(n => CreateChildren(n, 3, 3)).ToArray();
            //third level: 10°*10°
            var thirdLevel = secondLevel.SelectMany(n => CreateChildren(n, 3, 3)).ToArray();
            //fourth level: 5°*2°
            var fourthLevel = thirdLevel.SelectMany(n => CreateChildren(n, 2, 5)).ToArray();
            //fourth level: 1°*1°
            var fifthLevel = fourthLevel.SelectMany(n => CreateChildren(n, 5, 2)).ToArray();

            foreach (var tile in tiles)
            {
                SetTileToNode(root, tile);
            }

            //Shake tree
            root.RemoveEmptyChildren();

            return root;
        }

        private static IEnumerable<TileTreeNode> CreateChildren(TileTreeNode parentNode, int latitudeFraction, int longitudeFraction)
        {
            double latitudeSpan = parentNode.LatitudeSpan / latitudeFraction;
            double longitudeSpan = parentNode.LongitudeSpan / longitudeFraction;
            for (int i = 0; i < latitudeFraction; i++)
            {
                for (int j = 0; j < longitudeFraction; j++)
                {
                    double north = parentNode.North - i * latitudeSpan;
                    double west = parentNode.West + j * longitudeSpan;
                    var child = new TileTreeNode(north, west, latitudeSpan, longitudeSpan);
                    parentNode.AddChildren(child);
                    yield return child;
                }
            }
        }

        private static void SetTileToNode(TileTreeNode node, TileInfo tile)
        {
            if (!node.Intersect(tile))
                return;

            if (node.IsLeaf)
            {
                node.AddTile(tile);
                return;
            }

            foreach (var childNode in node.Children)
            {
                SetTileToNode(childNode, tile);
            }
        }
    }
}
