using System;
using System.Collections.Generic;
using NFluent;
using Xunit;

namespace NetElevation.Core.Test
{
    public class TileManagerTest
    {
        private class MockRepository : ITileRepository
        {
            public Func<TileInfo, short[]> GetElevationMapMock { get; set; } = _ => new short[0];
            public Func<TileInfo[]> GetTilesMock { get; set; } = () => new TileInfo[0];

            public short[] GetElevationMap(TileInfo tileInfo) => GetElevationMapMock(tileInfo);

            public TileInfo[] GetTiles() => GetTilesMock();
        }

        [Fact]
        public void GivenNoTilesThenGetElevationAlwaysReturnsZero()
        {
            var tileManager = new TileManager(new MockRepository());
            Check.That(tileManager.GetElevation(45, 5)).IsEqualTo(0);
        }

        [Theory]
        [InlineData(38, -90, 0)]
        [InlineData(45, 5, 1)]
        [InlineData(-15, -45, 2)]
        [InlineData(-45, 15, 3)]
        public void CheckThatTheRightTileIsLoaded(double latitude, double longitude, short expectedElevation)
        {
            TileInfo[] tiles =
            {
                new TileInfo(90, -180, 90, 180, 1,1) { FileName = "NW" },
                new TileInfo(90, 0, 90, 180, 1,1) { FileName = "NE" },
                new TileInfo(0, -180, 90, 180, 1,1) { FileName = "SW" },
                new TileInfo(0, 0, 90, 180, 1,1) { FileName = "SE" },
            };

            var elevationMapByTileName = new Dictionary<string, short[]>
            {
                ["NW"] = new short[] { 0 },
                ["NE"] = new short[] { 1 },
                ["SW"] = new short[] { 2 },
                ["SE"] = new short[] { 3 },
            };

            var repository = new MockRepository
            {
                GetTilesMock = () => tiles,
                GetElevationMapMock = tile => elevationMapByTileName[tile.FileName]
            };

            var tileManager = new TileManager(repository);
            Check.That(tileManager.GetElevation(latitude, longitude)).IsEqualTo(expectedElevation);
        }
    }
}
