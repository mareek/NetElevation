using System.Text.Json.Serialization;

namespace NetElevation.Core
{
    public abstract class BaseCoordinates
    {
        protected BaseCoordinates() { /*For serialization*/ }

        protected BaseCoordinates(double north, double west, double latitudeSpan, double longitudeSpan)
            : this()
        {
            North = north;
            West = west;
            LatitudeSpan = latitudeSpan;
            LongitudeSpan = longitudeSpan;
        }

        public double North { get; set; }
        public double West { get; }

        public double LatitudeSpan { get; set; }
        public double LongitudeSpan { get; set; }

        [JsonIgnore]
        public double South => North - LatitudeSpan;
        [JsonIgnore]
        public double East => West + LongitudeSpan;

        public bool Contains(double latitude, double longitude)
            => North >= latitude && latitude > South
                && East > longitude && longitude >= West;

        public bool Intersect(BaseCoordinates other)
            => North > other.South && South < other.North
                && West < other.East && East > other.West;
    }
}
