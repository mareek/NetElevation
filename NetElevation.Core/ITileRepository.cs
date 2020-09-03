namespace NetElevation.Core
{
    public interface ITileRepository
    {
        short[] LoadElevationMap(TileInfo tileInfo);
        TileInfo[] GetTiles();
    }
}