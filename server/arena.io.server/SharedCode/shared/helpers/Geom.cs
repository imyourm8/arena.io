using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared.helpers
{
    public struct Area
    {
        public float minX, maxX, minY, maxY;
        public Area(float minX_, float maxX_, float minY_, float maxY_)
        {
            minX = minX_;
            maxX = maxX_;
            minY = minY_;
            maxY = maxY_;
        }
        public float Width
        { get { return maxX - minX; } }

        public float Height
        { get { return maxY - minY; } }

        public bool IsInside(float x, float y)
        {
            return minX <= x && x <= maxX && minY <= y && y <= maxY;
        }

        public void RandomPoint(out float x, out float y)
        {
            x = minX + helpers.MathHelper.Range(0, Width);
            y = minY + helpers.MathHelper.Range(0, Height);
        }
    }

    public struct Vector2
    {
        public static readonly Vector2 zero = new Vector2(0, 0);

        public float x, y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2(double x, double y)
        {
            this.x = (float)x;
            this.y = (float)y;
        }

        public void Normilize()
        {
            float length = Length();
            if (length > 1.0f)
            {
                x /= length;
                y /= length;
            }
        }

        public void Scale(float s)
        {
            x *= s;
            y *= s;
        }

        public float Length()
        {
            return (float)Math.Sqrt(x * x + y * y);
        }

        public static Vector2 operator *(Vector2 other, float scale)
        {
            return new Vector2(other.x*scale, other.y*scale);
        }

        public static Vector2 operator+(Vector2 other, Vector2 other2)
        {
            return new Vector2(other.x+other2.x, other.y+other2.y);
        }

        public static Vector2 operator -(Vector2 other, Vector2 other2)
        {
            return new Vector2(other.x - other2.x, other.y - other2.y);
        }

        public static implicit operator Box2CS.Vec2(Vector2 toConvert)
        {
            return new Box2CS.Vec2(toConvert.x, toConvert.y);
        }

        public static implicit operator Vector2(Box2CS.Vec2 toConvert)
        {
            return new Vector2(toConvert.X, toConvert.Y);
        }
    }
}
