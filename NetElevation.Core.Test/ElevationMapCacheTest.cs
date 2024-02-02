using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NFluent;
using Xunit;

namespace NetElevation.Core.Test
{
    public class ElevationMapCacheTest
    {
        [Fact]
        public void TestBasicScenario()
        {
            var tile1 = new TileInfo { North = 1 };
            var tile2 = new TileInfo { North = 2 };
            var mapByTileName = new Dictionary<TileInfo, short[]>
            {
                [tile1] = new short[5],
                [tile2] = new short[10],
            };

            var repo = new MockRepository { GetElevationMapMock = tile => mapByTileName[tile] };
            var cache = new ElevationMapCache(repo, 1000);
            //warm cache
            Check.That(cache.GetValue(tile1)).IsEqualTo(mapByTileName[tile1]);
            Check.That(cache.GetValue(tile2)).IsEqualTo(mapByTileName[tile2]);

            //set repo to fail on load
            repo.GetElevationMapMock = _ => throw new Exception("at this point, all tiles should be loaded from cache");
            Check.That(cache.GetValue(tile1)).IsEqualTo(mapByTileName[tile1]);
            Check.That(cache.GetValue(tile2)).IsEqualTo(mapByTileName[tile2]);
        }

        [Fact]
        public void TestCacheInvalidation()
        {
            var tiles = Enumerable.Range(0, 5)
                                  .Select(i => new TileInfo { North = i })
                                  .ToArray();
            var mapByTile = tiles.ToDictionary(t => t, _ => new short[25]);
            var tileLoadCount = tiles.ToDictionary(t => t, _ => 0);

            var repo = new MockRepository
            {
                GetElevationMapMock = tile =>
                {
                    tileLoadCount[tile] = tileLoadCount[tile] + 1;
                    return mapByTile[tile];
                }
            };

            // create a cahce taht can hold 4 tiles
            var cache = new ElevationMapCache(repo, 200);

            // get the 4 first tiles repeatedly from the cache
            for (int j = 0; j < 10; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    var tile = tiles[i];
                    Check.That(cache.GetValue(tile)).IsEqualTo(mapByTile[tile]);
                }
            }
            // cache should contains tiles 0, 1, 2, 3

            // check that the tiles has only been loaded once
            Check.That(tileLoadCount[tiles[0]]).IsEqualTo(1);
            Check.That(tileLoadCount[tiles[1]]).IsEqualTo(1);
            Check.That(tileLoadCount[tiles[2]]).IsEqualTo(1);
            Check.That(tileLoadCount[tiles[3]]).IsEqualTo(1);
            Check.That(tileLoadCount[tiles[4]]).IsEqualTo(0);

            // get the 5th tile ; triggering a cache cleanup that'll set the cahce to 3/4 o the max size => 3 tiles
            Check.That(cache.GetValue(tiles[4])).IsEqualTo(mapByTile[tiles[4]]);
            // cache should contains tiles 2, 3, 4

            // get the 4 firt tiles again
            Check.That(cache.GetValue(tiles[3])).IsEqualTo(mapByTile[tiles[3]]);
            // cache should contains tiles 2, 3, 4
            Check.That(cache.GetValue(tiles[2])).IsEqualTo(mapByTile[tiles[2]]);
            // cache should contains tiles 2, 3, 4
            Check.That(cache.GetValue(tiles[1])).IsEqualTo(mapByTile[tiles[1]]);
            // cache should contains tiles 1, 2, 3, 4
            Check.That(cache.GetValue(tiles[0])).IsEqualTo(mapByTile[tiles[0]]);
            // cache should contains tiles 0, 1, 2

            // check the load count of each tile
            Check.That(tileLoadCount[tiles[4]]).IsEqualTo(1);
            Check.That(tileLoadCount[tiles[3]]).IsEqualTo(1);
            Check.That(tileLoadCount[tiles[2]]).IsEqualTo(1);
            Check.That(tileLoadCount[tiles[1]]).IsEqualTo(2);
            Check.That(tileLoadCount[tiles[0]]).IsEqualTo(2);
        }
    }
}
