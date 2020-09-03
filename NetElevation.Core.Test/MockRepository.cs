using System;

namespace NetElevation.Core.Test
{
    internal class MockRepository : ITileRepository
    {
        public Func<TileInfo, short[]> GetElevationMapMock { get; set; } = _ => new short[0];
        public Func<TileInfo[]> GetTilesMock { get; set; } = () => new TileInfo[0];

        public short[] LoadElevationMap(TileInfo tileInfo) => GetElevationMapMock(tileInfo);

        public TileInfo[] GetTiles() => GetTilesMock();
    }
}
