using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth_of_Secrets
{
    public struct Point : IEquatable<Point>
    {
        private static readonly Point zeroPoint;

        public int X;

        public int Y;

        public static Point Zero => zeroPoint;

        internal string DebugDisplayString => X + "  " + Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Point(int value)
        {
            X = value;
            Y = value;
        }

        public static Point operator +(Point value1, Point value2)
        {
            return new Point(value1.X + value2.X, value1.Y + value2.Y);
        }

        public static Point operator -(Point value1, Point value2)
        {
            return new Point(value1.X - value2.X, value1.Y - value2.Y);
        }

        public static Point operator *(Point value1, Point value2)
        {
            return new Point(value1.X * value2.X, value1.Y * value2.Y);
        }

        public static Point operator /(Point source, Point divisor)
        {
            return new Point(source.X / divisor.X, source.Y / divisor.Y);
        }

        public static bool operator ==(Point a, Point b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Point a, Point b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            if (obj is Point)
            {
                return Equals((Point)obj);
            }

            return false;
        }

        public bool Equals(Point other)
        {
            if (X == other.X)
            {
                return Y == other.Y;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (17 * 23 + X.GetHashCode()) * 23 + Y.GetHashCode();
        }

        public override string ToString()
        {
            return "{X:" + X + " Y:" + Y + "}";
        }

        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }

        public void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }
    }
}
