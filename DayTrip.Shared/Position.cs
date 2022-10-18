using System.Net;
using System.Text.Json.Serialization;
using static DayTrip.Shared.MathExt;

namespace DayTrip.Shared
{
    public record Position
    {
        [JsonConstructor]
        public Position(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }

        public static Position FromTuple((double, double) coords)
        {
            return new(coords.Item1, coords.Item2);
        }

        public double Lat { get; set; }
        public double Lon { get; set; }

        public (double, double) Deconstruct() => (Lat, Lon);
        public Position FloorRound() => new(DecimalFloor(Lat, 2), DecimalFloor(Lat, 2));
        public Position CeilRound() => new(DecimalCeil(Lat, 2), DecimalCeil(Lat, 2));

        public static Position operator *(Position a, (int, int) b) => new(a.Lat * b.Item1, a.Lat * b.Item2);
        public static Position operator +(Position a, Position b) => new(a.Lat + b.Lat, a.Lon + b.Lon);
    }
}