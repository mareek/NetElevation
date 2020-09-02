namespace NetElevation.Core
{
    public interface ITileRepository
    {
        short[] GetElevationMap(TileInfo tileInfo);
        TileInfo[] GetTiles();
    }
}