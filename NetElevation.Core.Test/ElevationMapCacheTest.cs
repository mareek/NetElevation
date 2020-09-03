using System;
using System.Collections.Generic;
using NFluent;
using Xunit;

namespace NetElevation.Core.Test
{
    public class ElevationMapCacheTest
    {
        [Fact]
        public void TestBasicScenario()
        {
            var tile1 = new TileInfo() { FileName = "tile1" };
            var tile2 = new TileInfo() { FileName = "tile2" };
            var mapByTileName = new Dictionary<string, short[]>
            {
                [tile1.FileName] = new short[5],
                [tile2.FileName] = new short[10],
            };

            var repo = new MockRepository { GetElevationMapMock = tile => mapByTileName[tile.FileName] };
            var cache = new ElevationMapCache(repo, 1000);
            //warm cache
            Check.That(cache.GetElevationMap(tile1)).IsEqualTo(mapByTileName[tile1.FileName]);
            Check.That(cache.GetElevationMap(tile2)).IsEqualTo(mapByTileName[tile2.FileName]);

            //set repo to fail on load
            repo.GetElevationMapMock = _ => throw new Exception("at this point, all tiles should be loaded from cache");
            Check.That(cache.GetElevationMap(tile1)).IsEqualTo(mapByTileName[tile1.FileName]);
            Check.That(cache.GetElevationMap(tile2)).IsEqualTo(mapByTileName[tile2.FileName]);
        }
    }
}
