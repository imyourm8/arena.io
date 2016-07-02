using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.helpers
{
    static class MathHelper
    {
        static Random random_ = new Random((int)CurrentTime.Instance.CurrentTimeInMs);
        public static float Range(float min, float max)
        {
            var d = random_.NextDouble();
            //d -= 0.5;
            //d *= 2.0;
            return (float)d * (max - min) + min;
        }

        public static int Range(int min, int max)
        {
            return random_.Next(min, max);
        }
    }
}
