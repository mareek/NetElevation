#nullable enable

namespace NetElevation.Core
{
    public class TileInfo : BaseCoordinates
    {
        public TileInfo() { /*For serialization*/ }

        public TileInfo(double north, double west, double latitudeSpan, double longitudeSpan, int width, int height, string? fileName = null)
            : base(north, west, latitudeSpan, longitudeSpan)
        {
            Width = width;
            Height = height;
            FileName = fileName;
        }

        public int Width { get; set; }
        public int Height { get; set; }

        public string? FileName { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is TileInfo info &&
                   North == info.North &&
                   West == info.West &&
                   LatitudeSpan == info.LatitudeSpan &&
                   LongitudeSpan == info.LongitudeSpan;
        }

        public short GetElevation(double latitude, double longitude, short[] elevationMap)
        {
            var offsetLatitude = North - latitude;
            var offsetLongitude = longitude - West;
            var x = (int)(Width * offsetLongitude / LongitudeSpan);
            var y = (int)(Height * offsetLatitude / LatitudeSpan);

            return elevationMap[x + y * Width];
        }

        public override int GetHashCode()
        {
            int hashCode = 114314577;
            hashCode = hashCode * -1521134295 + North.GetHashCode();
            hashCode = hashCode * -1521134295 + West.GetHashCode();
            hashCode = hashCode * -1521134295 + LatitudeSpan.GetHashCode();
            hashCode = hashCode * -1521134295 + LongitudeSpan.GetHashCode();
            return hashCode;
        }
    }
}
