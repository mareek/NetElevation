using NFluent;
using Xunit;

namespace NetElevation.Core.Test
{
    public class TileInfoTest
    {
        [Theory]
        [InlineData(0, 0, 1, true, 0, 0)]
        [InlineData(0, 0, 1, true, -0.5, 0.5)]
        [InlineData(0, 0, 1, false, 20, 20)]
        [InlineData(0, 0, 1, false, -1, 1)]
        [InlineData(5, -5, 10, true, -2, 2)]
        [InlineData(5, -5, 10, false, 10, 2)]
        public void TestContains(double north, double west, double span, bool expectedResult, double latitude, double longitude)
        {
            var tile = new TileInfo(north, west, span, span, 1, 1);
            Check.That(tile.Contains(latitude, longitude)).IsEqualTo(expectedResult);
        }

        [Theory]
        [InlineData(2, 0, 0)]
        [InlineData(1.5, 0.5, 0)]
        [InlineData(1.0001, 0.999, 0)]
        [InlineData(2, 1, 1)]
        [InlineData(1, 0, 2)]
        [InlineData(1, 1, 3)]
        public void TestGetElevation(double latitude, double longitude, short expectedElevation)
        {
            var tile = new TileInfo(2, 0, 2, 2, 2, 2);
            short[] elevationMap = { 0, 1,
                                     2, 3 };
            Check.That(tile.GetElevation(latitude, longitude, elevationMap)).IsEqualTo(expectedElevation);
        }

        [Theory]
        [InlineData(0, 0, 1, true, 0, 0, 1)]
        [InlineData(0, 0, 1, true, 0.5, 0.5, 1)]
        [InlineData(0, 0, 1, true, -0.5, 0.5, 1)]
        [InlineData(0, 0, 1, true, -0.5, 0.5, 0.1)]
        [InlineData(0, 0, 1, false, -1, 0, 1)]
        [InlineData(0, 0, 1, false, 0, 1, 1)]
        [InlineData(0, 0, 1, false, 5, 5, 1)]
        public void TestIntersect(double north1, double west1, double span1, bool expectedResult, double north2, double west2, double span2)
        {
            var tile1 = new TileInfo(north1, west1, span1, span1, 1, 1);
            var tile2 = new TileInfo(north2, west2, span2, span2, 1, 1);
            Check.That(tile1.Intersect(tile2)).IsEqualTo(expectedResult);
            Check.That(tile2.Intersect(tile1)).IsEqualTo(expectedResult);
        }

        [Fact]
        public void TestHashCode()
        {
            TestEqualAndHashCode(new TileInfo(0, 0, 1, 1, 1, 1), new TileInfo(0, 0, 1, 1, 1, 1), true);
            TestEqualAndHashCode(new TileInfo(0, 0, 1, 1, 1, 1), new TileInfo(0, 0, 1, 1, 10, 10), true);
            TestEqualAndHashCode(new TileInfo(0, 0, 1, 1, 1, 1), new TileInfo(0, 0, 2, 2, 1, 1), false);
            TestEqualAndHashCode(new TileInfo(0, 0, 1, 1, 1, 1), new TileInfo(1, 1, 1, 1, 1, 1), false);

            static void TestEqualAndHashCode(TileInfo referenceTile, TileInfo otherTile, bool isEqual)
            {
                Check.That(referenceTile.Equals(otherTile)).IsEqualTo(isEqual);
                Check.That(referenceTile.GetHashCode() == otherTile.GetHashCode()).IsEqualTo(isEqual);
            }
        }
    }
}
