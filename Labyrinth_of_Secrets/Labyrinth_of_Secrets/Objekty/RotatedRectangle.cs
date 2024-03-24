using Microsoft.Xna.Framework;
using System;

public class RotatedRectangle
{
    public Vector2 Center { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float Rotation { get; set; }

    public Vector2[] GetCorners()
    {
        Vector2[] corners = new Vector2[4];
        float halfWidth = Width / 2;
        float halfHeight = Height / 2;

        corners[0] = new Vector2(Center.X - halfWidth, Center.Y - halfHeight);
        corners[1] = new Vector2(Center.X + halfWidth, Center.Y - halfHeight);
        corners[2] = new Vector2(Center.X + halfWidth, Center.Y + halfHeight);
        corners[3] = new Vector2(Center.X - halfWidth, Center.Y + halfHeight);

        for (int i = 0; i < 4; i++)
        {
            Vector2 corner = corners[i];
            float rotatedX = Center.X + (corner.X - Center.X) * MathF.Cos(Rotation) - (corner.Y - Center.Y) * MathF.Sin(Rotation);
            float rotatedY = Center.Y + (corner.X - Center.X) * MathF.Sin(Rotation) + (corner.Y - Center.Y) * MathF.Cos(Rotation);
            corners[i] = new Vector2(rotatedX, rotatedY);
        }

        return corners;
    }

    public bool Intersects(RotatedRectangle other)
    {
        Vector2[] corners1 = GetCorners();
        Vector2[] corners2 = other.GetCorners();

        for (int i = 0; i < 4; i++)
        {
            Vector2 axis = new Vector2(corners1[i].Y - corners1[(i + 1) % 4].Y, corners1[(i + 1) % 4].X - corners1[i].X);
            if (!OverlapOnAxis(axis, corners1, corners2))
            {
                return false;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            Vector2 axis = new Vector2(corners2[i].Y - corners2[(i + 1) % 4].Y, corners2[(i + 1) % 4].X - corners2[i].X);
            if (!OverlapOnAxis(axis, corners1, corners2))
            {
                return false;
            }
        }

        return true;
    }

    private bool OverlapOnAxis(Vector2 axis, Vector2[] corners1, Vector2[] corners2)
    {
        float min1 = Vector2.Dot(axis, corners1[0]);
        float max1 = min1;
        for (int i = 1; i < 4; i++)
        {
            float value = Vector2.Dot(axis, corners1[i]);
            min1 = MathF.Min(min1, value);
            max1 = MathF.Max(max1, value);
        }

        float min2 = Vector2.Dot(axis, corners2[0]);
        float max2 = min2;
        for (int i = 1; i < 4; i++)
        {
            float value = Vector2.Dot(axis, corners2[i]);
            min2 = MathF.Min(min2, value);
            max2 = MathF.Max(max2, value);
        }

        if (min1 < min2)
        {
            return max1 >= min2;
        }
        else
        {
            return min1 <= max2;
        }
    }
}
