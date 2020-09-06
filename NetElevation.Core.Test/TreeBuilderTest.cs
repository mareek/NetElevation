using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using NFluent;
using Xunit;

namespace NetElevation.Core.Test
{
    public class TreeBuilderTest
    {
        [Fact]
        public void TestEmptyTree()
        {
            var emptyRoot = TreeBuilder.BuildTree(new TileInfo[0]);
            Check.That(emptyRoot.IsEmpty).IsTrue();
        }

        [Fact]
        public void TestTreeWithTwoTiles()
        {
            var lyonTile = new TileInfo(50, 0, 5, 5, 1, 1);
            var machuPicchuTile = new TileInfo(-10, -75, 5, 5, 1, 1);
            var root = TreeBuilder.BuildTree(new[] { lyonTile, machuPicchuTile });

            Check.That(root.IsEmpty).IsFalse();
            Check.That(root.GetTile(45.75, 4.85)).IsEqualTo(lyonTile);
            Check.That(root.GetTile(0, 0)).IsNull();
            Check.That(root.GetTile(-13.163, -72.545)).IsEqualTo(machuPicchuTile);
        }
    }
}
