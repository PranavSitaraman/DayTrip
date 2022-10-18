using System;

namespace DayTrip.Shared
{
    public class MathExt
    {
        public static readonly (int, int)[] Neighbors =
        {
            (-1, 1), (0, 1), (1, 1),
            (-1, 0), (0, 0), (1, 0),
            (-1, -1), (0, -1), (1, -1),
        };

        public static readonly double TileSize = 0.01;
        public static double DecimalFloor(double original, int precision)
        {
            int factor = (int) Math.Pow(10, precision);
            original *= factor;
            original = Math.Floor(original);
            original /= factor;
            return original;
        }

        public static double DecimalCeil(double original, int precision)
        {
            int factor = (int) Math.Pow(10, precision);
            original *= factor;
            original = Math.Ceiling(original);
            original /= factor;
            return original;
        }
    }
}