#nullable enable

namespace NetElevation.Core
{
    public class TileInfo : BaseCoordinates
    {
        public TileInfo() { /*For serialization*/ }

        public TileInfo(double north, double west, double latitudeSpan, double longitudeSpan, int width, int height)
            : base(north, west, latitudeSpan, longitudeSpan)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; set; }
        public int Height { get; set; }

        public string? FileName { get; set; }

        public short GetElevation(double latitude, double longitude, short[] elevationMap)
        {
            var offsetLatitude = North - latitude;
            var offsetLongitude = longitude - West;
            var x = (int)(Width * offsetLongitude / LongitudeSpan);
            var y = (int)(Height * offsetLatitude / LatitudeSpan);

            return elevationMap[x + y * Width];
        }
    }
}
