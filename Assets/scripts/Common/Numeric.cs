using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TapCommon
{
    /* stores any of types
     * (int, long, float, double)
     */
   /* public struct Numeric
    {
        private byte[] data_;

        public void Store(int v)
        {
            var bytes = BitConverter.GetBytes(v);
            Array.Copy(bytes, 0, data_, 0, bytes.LongLength);
        }

        public void Store(long v)
        {
            var bytes = BitConverter.GetBytes(v);
            Array.Copy(bytes, 0, data_, 0, bytes.LongLength);
        }

        public void Store(float v)
        {
            var bytes = BitConverter.GetBytes(v);
            Array.Copy(bytes, 0, data_, 0, bytes.LongLength);
        }

        public void Store(double v)
        {
            var bytes = BitConverter.GetBytes(v);
            Array.Copy(bytes, 0, data_, 0, bytes.LongLength);
        }

        public double AsDouble()
        {
            return BitConverter.ToDouble(data_, 0);
        }

        public float AsSingle()
        {
            return BitConverter.ToSingle(data_, 0);
        }

        public float AsInt()
        {
            return BitConverter.ToInt32(data_, 0);
        }

        public float AsLong()
        {
            return BitConverter.ToInt64(data_, 0);
        }
    }*/
}
