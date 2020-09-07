using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Transactions;

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

            SetTilesToNodes(fifthLevel, tiles);

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

        private static void SetTilesToNodes(TileTreeNode[] leafNodes, TileInfo[] tiles)
        {
            var northLimit = tiles.Max(tiles => tiles.North);
            var southLimit = tiles.Min(tiles => tiles.South);

            var sortedTiles = tiles.OrderBy(t => t.West).ToArray();
            TileInfo[] GetNodeTiles(TileTreeNode node)
                => sortedTiles.SkipWhile(t => t.East < node.West)
                              .TakeWhile(t => t.West < node.East)
                              .Where(t => t.Intersect(node))
                              .ToArray();

            leafNodes.AsParallel()
                     .Where(n => northLimit > n.South && n.North > southLimit)
                     .ForAll(node => node.AddTiles(GetNodeTiles(node)));
        }

        private static void SetTilesToNodesAlt(TileTreeNode[] leafNodes, TileInfo[] tiles)
        {
            var sortedTiles = tiles.OrderBy(t => t.West).ToArray();

            Span<TileInfo> GetCandidateTiles(TileTreeNode node)
            {
                int start = 0;
                int finish = sortedTiles.Length-1;
                return sortedTiles.AsSpan(start, finish - start + 1);
            }

            void AssignNodeTiles(TileTreeNode node)
            {
                foreach (var tile in GetCandidateTiles(node))
                {
                    if (tile.Intersect(node))
                    {
                        node.AddTile(tile);
                    }
                }
            }

            var northLimit = tiles.Max(tiles => tiles.North);
            var southLimit = tiles.Min(tiles => tiles.South);

            leafNodes.AsParallel()
                     .Where(n => northLimit > n.South && n.North > southLimit)
                     .ForAll(AssignNodeTiles);
        }
    }
}
