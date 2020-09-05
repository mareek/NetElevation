using NFluent;
using Xunit;

namespace NetElevation.Core.Test
{
    public class TileTreeNodeTest
    {
        [Fact]
        public void TestGetTile()
        {
            var emptyNode = new TileTreeNode(10, 10, 10, 10);
            Check.That(emptyNode.GetTile(10, 10)).IsNull();

            var singleTileNode = new TileTreeNode(45, 45, 10, 10);
            var lonelyTile = new TileInfo(45, 45, 10, 10, 1, 1);
            singleTileNode.AddTiles(lonelyTile);
            Check.That(singleTileNode.GetTile(-15, -15)).IsNull();
            Check.That(singleTileNode.GetTile(45, 45)).IsEqualTo(lonelyTile);
            Check.That(singleTileNode.GetTile(40, 50)).IsEqualTo(lonelyTile);

            var twinTileNode = new TileTreeNode(30, 30, 5, 10);
            var twinTile1 = new TileInfo(30, 30, 5, 5, 1, 1);
            var twinTile2 = new TileInfo(30, 35, 5, 5, 1, 1);
            twinTileNode.AddTiles(twinTile1, twinTile2);
            Check.That(twinTileNode.GetTile(30, 30)).IsEqualTo(twinTile1);
            Check.That(twinTileNode.GetTile(30, 35)).IsEqualTo(twinTile2);

            var root = new TileTreeNode(90, -180, 180, 360);
            root.AddChildren(singleTileNode, emptyNode, twinTileNode);

            Check.That(root.GetTile(10, 10)).IsNull();
            Check.That(root.GetTile(-15, -15)).IsNull();
            Check.That(root.GetTile(45, 45)).IsEqualTo(lonelyTile);
            Check.That(root.GetTile(40, 50)).IsEqualTo(lonelyTile);
            Check.That(root.GetTile(30, 30)).IsEqualTo(twinTile1);
            Check.That(root.GetTile(30, 35)).IsEqualTo(twinTile2);
        }
    }
}
