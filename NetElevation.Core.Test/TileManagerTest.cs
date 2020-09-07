using System.Collections.Generic;
using NFluent;
using Xunit;

namespace NetElevation.Core.Test
{
    public class TileManagerTest
    {
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

        [Fact]
        public void TestSetElevation()
        {
            var lyonTile = new TileInfo(50, 0, 5, 5, 1, 1);
            var machuPicchuTile = new TileInfo(-10, -75, 5, 5, 1, 1);
            var elevationMapByTile = new Dictionary<TileInfo, short[]>
            {
                [lyonTile] = new short[] { 200 },
                [machuPicchuTile] = new short[] { 2500 },
            };

            var lyonLocation = new Location { Latitude = 45.75, Longitude = 4.85 };
            var trevouxLocation = new Location { Latitude = 45.94, Longitude = 4.77 };
            var machuPicchuLocation = new Location { Latitude = -13.163, Longitude = -72.545 };
            var newYorkLocation = new Location { Latitude = 40.8, Longitude = -73.97 };
            Location[] locations = { lyonLocation, machuPicchuLocation, newYorkLocation, trevouxLocation };

            var selfDestructRepo = new MockRepository
            {
                GetTilesMock = () => new[] { lyonTile, machuPicchuTile },
                GetElevationMapMock = tile =>
                {
                    var result = elevationMapByTile[tile];
                    elevationMapByTile.Remove(tile);
                    return result;
                }
            };

            var tileManager = new TileManager(selfDestructRepo, 0);

            tileManager.SetElevations(locations);

            Check.That(lyonLocation.Elevation).IsEqualTo(200);
            Check.That(trevouxLocation.Elevation).IsEqualTo(200);
            Check.That(machuPicchuLocation.Elevation).IsEqualTo(2500);
            Check.That(newYorkLocation.Elevation).IsEqualTo(0);
        }
    }
}
