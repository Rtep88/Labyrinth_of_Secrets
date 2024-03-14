using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth_of_Secrets
{
    public struct Vector2 : IEquatable<Vector2>
    {
        private static readonly Vector2 zeroVector = new Vector2(0f, 0f);

        private static readonly Vector2 unitVector = new Vector2(1f, 1f);

        private static readonly Vector2 unitXVector = new Vector2(1f, 0f);

        private static readonly Vector2 unitYVector = new Vector2(0f, 1f);

        public float X;

        public float Y;

        public static Vector2 Zero => zeroVector;

        public static Vector2 One => unitVector;

        public static Vector2 UnitX => unitXVector;

        public static Vector2 UnitY => unitYVector;

        internal string DebugDisplayString => X + "  " + Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public Vector2(float value)
        {
            X = value;
            Y = value;
        }

        public static implicit operator Vector2(System.Numerics.Vector2 value)
        {
            return new Vector2(value.X, value.Y);
        }

        public static Vector2 operator -(Vector2 value)
        {
            value.X = 0f - value.X;
            value.Y = 0f - value.Y;
            return value;
        }

        public static Vector2 operator +(Vector2 value1, Vector2 value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            return value1;
        }

        public static Vector2 operator -(Vector2 value1, Vector2 value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            return value1;
        }

        public static Vector2 operator *(Vector2 value1, Vector2 value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            return value1;
        }

        public static Vector2 operator *(Vector2 value, float scaleFactor)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            return value;
        }

        public static Vector2 operator *(float scaleFactor, Vector2 value)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            return value;
        }

        public static Vector2 operator /(Vector2 value1, Vector2 value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            return value1;
        }

        public static Vector2 operator /(Vector2 value1, float divider)
        {
            float num = 1f / divider;
            value1.X *= num;
            value1.Y *= num;
            return value1;
        }

        public static bool operator ==(Vector2 value1, Vector2 value2)
        {
            if (value1.X == value2.X)
            {
                return value1.Y == value2.Y;
            }

            return false;
        }

        public static bool operator !=(Vector2 value1, Vector2 value2)
        {
            if (value1.X == value2.X)
            {
                return value1.Y != value2.Y;
            }

            return true;
        }

        public static Vector2 Add(Vector2 value1, Vector2 value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            return value1;
        }

        public static void Add(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
        {
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
        }

        public void Ceiling()
        {
            X = MathF.Ceiling(X);
            Y = MathF.Ceiling(Y);
        }

        public static Vector2 Ceiling(Vector2 value)
        {
            value.X = MathF.Ceiling(value.X);
            value.Y = MathF.Ceiling(value.Y);
            return value;
        }

        public static void Ceiling(ref Vector2 value, out Vector2 result)
        {
            result.X = MathF.Ceiling(value.X);
            result.Y = MathF.Ceiling(value.Y);
        }

        public static float Distance(Vector2 value1, Vector2 value2)
        {
            float num = value1.X - value2.X;
            float num2 = value1.Y - value2.Y;
            return MathF.Sqrt(num * num + num2 * num2);
        }

        public static void Distance(ref Vector2 value1, ref Vector2 value2, out float result)
        {
            float num = value1.X - value2.X;
            float num2 = value1.Y - value2.Y;
            result = MathF.Sqrt(num * num + num2 * num2);
        }

        public static float DistanceSquared(Vector2 value1, Vector2 value2)
        {
            float num = value1.X - value2.X;
            float num2 = value1.Y - value2.Y;
            return num * num + num2 * num2;
        }

        public static void DistanceSquared(ref Vector2 value1, ref Vector2 value2, out float result)
        {
            float num = value1.X - value2.X;
            float num2 = value1.Y - value2.Y;
            result = num * num + num2 * num2;
        }

        public static Vector2 Divide(Vector2 value1, Vector2 value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            return value1;
        }

        public static void Divide(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
        {
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
        }

        public static Vector2 Divide(Vector2 value1, float divider)
        {
            float num = 1f / divider;
            value1.X *= num;
            value1.Y *= num;
            return value1;
        }

        public static void Divide(ref Vector2 value1, float divider, out Vector2 result)
        {
            float num = 1f / divider;
            result.X = value1.X * num;
            result.Y = value1.Y * num;
        }

        public static float Dot(Vector2 value1, Vector2 value2)
        {
            return value1.X * value2.X + value1.Y * value2.Y;
        }

        public static void Dot(ref Vector2 value1, ref Vector2 value2, out float result)
        {
            result = value1.X * value2.X + value1.Y * value2.Y;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2)
            {
                return Equals((Vector2)obj);
            }

            return false;
        }

        public bool Equals(Vector2 other)
        {
            if (X == other.X)
            {
                return Y == other.Y;
            }

            return false;
        }

        public void Floor()
        {
            X = MathF.Floor(X);
            Y = MathF.Floor(Y);
        }

        public static Vector2 Floor(Vector2 value)
        {
            value.X = MathF.Floor(value.X);
            value.Y = MathF.Floor(value.Y);
            return value;
        }

        public static void Floor(ref Vector2 value, out Vector2 result)
        {
            result.X = MathF.Floor(value.X);
            result.Y = MathF.Floor(value.Y);
        }

        public override int GetHashCode()
        {
            return (X.GetHashCode() * 397) ^ Y.GetHashCode();
        }

        public float Length()
        {
            return MathF.Sqrt(X * X + Y * Y);
        }

        public float LengthSquared()
        {
            return X * X + Y * Y;
        }

        public static Vector2 Max(Vector2 value1, Vector2 value2)
        {
            return new Vector2((value1.X > value2.X) ? value1.X : value2.X, (value1.Y > value2.Y) ? value1.Y : value2.Y);
        }

        public static void Max(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
        {
            result.X = ((value1.X > value2.X) ? value1.X : value2.X);
            result.Y = ((value1.Y > value2.Y) ? value1.Y : value2.Y);
        }

        public static Vector2 Min(Vector2 value1, Vector2 value2)
        {
            return new Vector2((value1.X < value2.X) ? value1.X : value2.X, (value1.Y < value2.Y) ? value1.Y : value2.Y);
        }

        public static void Min(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
        {
            result.X = ((value1.X < value2.X) ? value1.X : value2.X);
            result.Y = ((value1.Y < value2.Y) ? value1.Y : value2.Y);
        }

        public static Vector2 Multiply(Vector2 value1, Vector2 value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            return value1;
        }

        public static void Multiply(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
        {
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
        }

        public static Vector2 Multiply(Vector2 value1, float scaleFactor)
        {
            value1.X *= scaleFactor;
            value1.Y *= scaleFactor;
            return value1;
        }

        public static void Multiply(ref Vector2 value1, float scaleFactor, out Vector2 result)
        {
            result.X = value1.X * scaleFactor;
            result.Y = value1.Y * scaleFactor;
        }

        public static Vector2 Negate(Vector2 value)
        {
            value.X = 0f - value.X;
            value.Y = 0f - value.Y;
            return value;
        }

        public static void Negate(ref Vector2 value, out Vector2 result)
        {
            result.X = 0f - value.X;
            result.Y = 0f - value.Y;
        }

        public void Normalize()
        {
            float num = 1f / MathF.Sqrt(X * X + Y * Y);
            X *= num;
            Y *= num;
        }

        public static Vector2 Normalize(Vector2 value)
        {
            float num = 1f / MathF.Sqrt(value.X * value.X + value.Y * value.Y);
            value.X *= num;
            value.Y *= num;
            return value;
        }

        public static void Normalize(ref Vector2 value, out Vector2 result)
        {
            float num = 1f / MathF.Sqrt(value.X * value.X + value.Y * value.Y);
            result.X = value.X * num;
            result.Y = value.Y * num;
        }

        public static Vector2 Reflect(Vector2 vector, Vector2 normal)
        {
            float num = 2f * (vector.X * normal.X + vector.Y * normal.Y);
            Vector2 result = default(Vector2);
            result.X = vector.X - normal.X * num;
            result.Y = vector.Y - normal.Y * num;
            return result;
        }

        public static void Reflect(ref Vector2 vector, ref Vector2 normal, out Vector2 result)
        {
            float num = 2f * (vector.X * normal.X + vector.Y * normal.Y);
            result.X = vector.X - normal.X * num;
            result.Y = vector.Y - normal.Y * num;
        }

        public void Round()
        {
            X = MathF.Round(X);
            Y = MathF.Round(Y);
        }

        public static Vector2 Round(Vector2 value)
        {
            value.X = MathF.Round(value.X);
            value.Y = MathF.Round(value.Y);
            return value;
        }

        public static void Round(ref Vector2 value, out Vector2 result)
        {
            result.X = MathF.Round(value.X);
            result.Y = MathF.Round(value.Y);
        }

        public static Vector2 Subtract(Vector2 value1, Vector2 value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            return value1;
        }

        public static void Subtract(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
        {
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
        }

        public override string ToString()
        {
            return "{X:" + X + " Y:" + Y + "}";
        }

        public Point ToPoint()
        {
            return new Point((int)X, (int)Y);
        }

        public void Deconstruct(out float x, out float y)
        {
            x = X;
            y = Y;
        }

        public System.Numerics.Vector2 ToNumerics()
        {
            return new System.Numerics.Vector2(X, Y);
        }
    }
}
