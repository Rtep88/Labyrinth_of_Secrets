namespace Labyrinth_of_Secrets
{
    public struct Rectangle : IEquatable<Rectangle>
    {
        private static Rectangle emptyRectangle;

        public int X;

        public int Y;

        public int Width;

        public int Height;

        public static Rectangle Empty => emptyRectangle;

        public int Left => X;

        public int Right => X + Width;

        public int Top => Y;

        public int Bottom => Y + Height;

        public bool IsEmpty
        {
            get
            {
                if (Width == 0 && Height == 0 && X == 0)
                {
                    return Y == 0;
                }

                return false;
            }
        }

        public Point Location
        {
            get
            {
                return new Point(X, Y);
            }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public Point Size
        {
            get
            {
                return new Point(Width, Height);
            }
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }

        public Point Center => new Point(X + Width / 2, Y + Height / 2);

        internal string DebugDisplayString => X + "  " + Y + "  " + Width + "  " + Height;

        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectangle(Point location, Point size)
        {
            X = location.X;
            Y = location.Y;
            Width = size.X;
            Height = size.Y;
        }

        public static bool operator ==(Rectangle a, Rectangle b)
        {
            if (a.X == b.X && a.Y == b.Y && a.Width == b.Width)
            {
                return a.Height == b.Height;
            }

            return false;
        }

        public static bool operator !=(Rectangle a, Rectangle b)
        {
            return !(a == b);
        }

        public bool Contains(int x, int y)
        {
            if (X <= x && x < X + Width && Y <= y)
            {
                return y < Y + Height;
            }

            return false;
        }

        public bool Contains(float x, float y)
        {
            if ((float)X <= x && x < (float)(X + Width) && (float)Y <= y)
            {
                return y < (float)(Y + Height);
            }

            return false;
        }

        public bool Contains(Point value)
        {
            if (X <= value.X && value.X < X + Width && Y <= value.Y)
            {
                return value.Y < Y + Height;
            }

            return false;
        }

        public void Contains(ref Point value, out bool result)
        {
            result = X <= value.X && value.X < X + Width && Y <= value.Y && value.Y < Y + Height;
        }

        public bool Contains(Vector2 value)
        {
            if ((float)X <= value.X && value.X < (float)(X + Width) && (float)Y <= value.Y)
            {
                return value.Y < (float)(Y + Height);
            }

            return false;
        }

        public void Contains(ref Vector2 value, out bool result)
        {
            result = (float)X <= value.X && value.X < (float)(X + Width) && (float)Y <= value.Y && value.Y < (float)(Y + Height);
        }

        public bool Contains(Rectangle value)
        {
            if (X <= value.X && value.X + value.Width <= X + Width && Y <= value.Y)
            {
                return value.Y + value.Height <= Y + Height;
            }

            return false;
        }

        public void Contains(ref Rectangle value, out bool result)
        {
            result = X <= value.X && value.X + value.Width <= X + Width && Y <= value.Y && value.Y + value.Height <= Y + Height;
        }

        public override bool Equals(object obj)
        {
            if (obj is Rectangle)
            {
                return this == (Rectangle)obj;
            }

            return false;
        }

        public bool Equals(Rectangle other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return (((17 * 23 + X.GetHashCode()) * 23 + Y.GetHashCode()) * 23 + Width.GetHashCode()) * 23 + Height.GetHashCode();
        }

        public void Inflate(int horizontalAmount, int verticalAmount)
        {
            X -= horizontalAmount;
            Y -= verticalAmount;
            Width += horizontalAmount * 2;
            Height += verticalAmount * 2;
        }

        public void Inflate(float horizontalAmount, float verticalAmount)
        {
            X -= (int)horizontalAmount;
            Y -= (int)verticalAmount;
            Width += (int)horizontalAmount * 2;
            Height += (int)verticalAmount * 2;
        }

        public bool Intersects(Rectangle value)
        {
            if (value.Left < Right && Left < value.Right && value.Top < Bottom)
            {
                return Top < value.Bottom;
            }

            return false;
        }

        public void Intersects(ref Rectangle value, out bool result)
        {
            result = value.Left < Right && Left < value.Right && value.Top < Bottom && Top < value.Bottom;
        }

        public static Rectangle Intersect(Rectangle value1, Rectangle value2)
        {
            Intersect(ref value1, ref value2, out var result);
            return result;
        }

        public static void Intersect(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
        {
            if (value1.Intersects(value2))
            {
                int num = Math.Min(value1.X + value1.Width, value2.X + value2.Width);
                int num2 = Math.Max(value1.X, value2.X);
                int num3 = Math.Max(value1.Y, value2.Y);
                int num4 = Math.Min(value1.Y + value1.Height, value2.Y + value2.Height);
                result = new Rectangle(num2, num3, num - num2, num4 - num3);
            }
            else
            {
                result = new Rectangle(0, 0, 0, 0);
            }
        }

        public void Offset(int offsetX, int offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        public void Offset(float offsetX, float offsetY)
        {
            X += (int)offsetX;
            Y += (int)offsetY;
        }

        public void Offset(Point amount)
        {
            X += amount.X;
            Y += amount.Y;
        }

        public void Offset(Vector2 amount)
        {
            X += (int)amount.X;
            Y += (int)amount.Y;
        }

        public override string ToString()
        {
            return "{X:" + X + " Y:" + Y + " Width:" + Width + " Height:" + Height + "}";
        }

        public static Rectangle Union(Rectangle value1, Rectangle value2)
        {
            int num = Math.Min(value1.X, value2.X);
            int num2 = Math.Min(value1.Y, value2.Y);
            return new Rectangle(num, num2, Math.Max(value1.Right, value2.Right) - num, Math.Max(value1.Bottom, value2.Bottom) - num2);
        }

        public static void Union(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
        {
            result.X = Math.Min(value1.X, value2.X);
            result.Y = Math.Min(value1.Y, value2.Y);
            result.Width = Math.Max(value1.Right, value2.Right) - result.X;
            result.Height = Math.Max(value1.Bottom, value2.Bottom) - result.Y;
        }

        public void Deconstruct(out int x, out int y, out int width, out int height)
        {
            x = X;
            y = Y;
            width = Width;
            height = Height;
        }
    }
}