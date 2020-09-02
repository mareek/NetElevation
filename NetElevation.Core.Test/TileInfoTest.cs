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
    }
}
