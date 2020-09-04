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
            var tile1 = new TileInfo { FileName = "tile1" };
            var tile2 = new TileInfo { FileName = "tile2" };
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

        [Fact]
        public void TestCacheInvalidation()
        {
            var tiles = Enumerable.Range(0, 5)
                                  .Select(i => new TileInfo { FileName = $"tile_{i}" })
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

            var cache = new ElevationMapCache(repo, 200);
            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    var tile = tiles[i];
                    Check.That(cache.GetElevationMap(tile)).IsEqualTo(mapByTile[tile]);
                    Thread.Sleep(1);
                }
            }

            Check.That(tileLoadCount[tiles[0]]).IsEqualTo(1);
            Check.That(tileLoadCount[tiles[1]]).IsEqualTo(1);
            Check.That(tileLoadCount[tiles[2]]).IsEqualTo(1);
            Check.That(tileLoadCount[tiles[3]]).IsEqualTo(1);
            Check.That(tileLoadCount[tiles[4]]).IsEqualTo(0);

            Check.That(cache.GetElevationMap(tiles[4])).IsEqualTo(mapByTile[tiles[4]]);
            Check.That(cache.GetElevationMap(tiles[3])).IsEqualTo(mapByTile[tiles[3]]);
            Check.That(cache.GetElevationMap(tiles[2])).IsEqualTo(mapByTile[tiles[2]]);
            Check.That(cache.GetElevationMap(tiles[1])).IsEqualTo(mapByTile[tiles[1]]);
            Check.That(cache.GetElevationMap(tiles[0])).IsEqualTo(mapByTile[tiles[0]]);

            Check.That(tileLoadCount[tiles[4]]).IsEqualTo(1);
            Check.That(tileLoadCount[tiles[3]]).IsEqualTo(1);
            Check.That(tileLoadCount[tiles[2]]).IsEqualTo(1);
            Check.That(tileLoadCount[tiles[0]]).IsEqualTo(2);
            Check.That(tileLoadCount[tiles[1]]).IsEqualTo(2);

        }
    }
}
