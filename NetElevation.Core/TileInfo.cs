#nullable enable
using System.Text.Json.Serialization;

namespace NetElevation.Core
{
    public class TileInfo
    {
        public TileInfo() { /*For serialization*/ }

        public TileInfo(double north, double west, double latituteSpan, double longitudeSpan, int width, int height)
        {
            North = north;
            West = west;
            LatitudeSpan = latituteSpan;
            LongitudeSpan = longitudeSpan;
            Width = width;
            Height = height;
        }

        public double North { get; set; }
        public double West { get; set; }

        public double LatitudeSpan { get; set; }
        public double LongitudeSpan { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public string? FileName { get; set; }

        [JsonIgnore]
        public double South => North - LatitudeSpan;
        [JsonIgnore]
        public double East => West + LongitudeSpan;

        public bool Contains(double latitude, double longitude)
            => North >= latitude && latitude > South
                && East > longitude && longitude >= West;

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
