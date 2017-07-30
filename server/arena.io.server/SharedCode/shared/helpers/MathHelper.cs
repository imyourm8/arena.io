using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared.helpers
{
    public static class MathHelper
    {
        public static readonly float Deg2Rad = 0.0174533f;
        static Random random_ = new Random((int)CurrentTime.Instance.CurrentTimeInMs);
        public static float Range(float min, float max)
        {
            var d = random_.NextDouble();
            return (float)d * (max - min) + min;
        }

        public static int Range(int min, int max)
        {
            return random_.Next(min, max);
        }

        public static bool Approx(float x, float y)
        {
            return Math.Abs(x - y) <= 0.0001f;
        }

        public static bool Approx(Vector2 v1, Vector2 v2)
        {
            return Approx(v1.x, v2.x) && Approx(v1.y, v2.y);
        }

        public static float Distance(float x, float y, float x2, float y2)
        {
            return (float)Math.Sqrt((x-x2)*(x-x2) + (y-y2)*(y-y2));
        }

        public static float Distance(Vector2 v1, Vector2 v2)
        {
            return Distance(v1.x, v1.y, v2.x, v2.y);
        }

        public static float Clamp01(float val)
        {
            return Clamp(val, 0, 1);
        }

        public static float Clamp(float val, float min, float max)
        { 
            val = val < min ? min : val;
            val = val > max ? max : val;
            return val;
        }

        public static Vector2 RandomPointInsideCircle(Vector2 pos, float radius)
        {
            Vector2 point = pos;
            float angle = Range(0, 360) * Deg2Rad;
            radius = Range(0, radius);
            point.x = (float)Math.Cos(angle) + pos.x;
            point.y = (float)Math.Sin(angle) + pos.y;
            return point;
        }
    }
}
