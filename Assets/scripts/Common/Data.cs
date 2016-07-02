using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TapCommon
{
    public class NumericData
    {
        private byte[] fields_;
        private static readonly int BYTES_PER_FIELD = 8;
        private uint fieldCount_ = 0;

        public NumericData(uint count)
        {
            fieldCount_ = count;
            fields_ = new byte[fieldCount_ * BYTES_PER_FIELD];
        }

        public float FloatValue(uint field)
        {
            Trace.Assert(field < fieldCount_);
            var index = field * BYTES_PER_FIELD;
            return BitConverter.ToSingle(fields_, (int)index);
        }

        public void SetFloatValue(uint field, float value)
        {
            var index = field * BYTES_PER_FIELD;
            var bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, fields_, index, bytes.LongLength);
        }

        public double DoubleValue(uint field)
        {
            Trace.Assert(field < fieldCount_);
            var index = field * BYTES_PER_FIELD;
            return BitConverter.ToDouble(fields_, (int)index);
        }

        public void SetDoubleValue(uint field, double value)
        {
            var index = field * BYTES_PER_FIELD;
            var bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, fields_, index, bytes.LongLength);
        }

        public int IntValue(uint field)
        {
            Trace.Assert(field < fieldCount_);
            var index = field * BYTES_PER_FIELD;
            return BitConverter.ToInt32(fields_, (int)index);
        }

        public void SetIntValue(uint field, int value)
        {
            var index = field * BYTES_PER_FIELD;
            var bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, fields_, index, bytes.LongLength);
        }

        public long LongValue(uint field)
        {
            Trace.Assert(field < fieldCount_);
            var index = field * BYTES_PER_FIELD;
            return BitConverter.ToInt64(fields_, (int)index);
        }

        public void SetLongValue(uint field, long value)
        {
            var index = field * BYTES_PER_FIELD;
            var bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, fields_, index, bytes.LongLength);
        }
    }

    public class FlagData
    {
        private uint flagCount_ = 0;
        private byte[] flags_;
        public FlagData(uint total)
        {
            flagCount_ = total;
            flags_ = new byte[(uint)Math.Ceiling((double)flagCount_ / 8.0)];
        }

        public void SetFlag(int flag, bool val)
        {
            int bit = flag / 8;
            int bitIndex = flag % 8;
            if (val)
            {
                flags_[bit] |= (byte)(1 << bitIndex);
            }
            else
            {
                flags_[bit] &= (byte)~(1 << bitIndex);
            }
        }

        public bool HasFlag(int flag)
        {
            int bit = flag / 8;
            int bitIndex = flag % 8;
            return (flags_[bit] & (byte)(1 << bitIndex)) != 0;
        }
    }
        
    public class Data
    {
        public uint fieldCount_ = 0;
        public uint flagCount_ = 0;

        private NumericData numeric_;
        private FlagData flagData_;

        public void initFieldsAndFlags()
        {
			if (numeric_== null)
            numeric_ = new NumericData(fieldCount_);

			if (flagData_==null)
            flagData_ = new FlagData(flagCount_);
        }

        public void SetFlag(int flag, bool val)
        {
            flagData_.SetFlag(flag, val);
        }

        public bool HasFlag(int flag)
        {
            return flagData_.HasFlag(flag);
        }

        public float FloatValue(uint field)
        {
            return numeric_.FloatValue(field);
        }

        public void SetFloatValue(uint field, float value)
        {
            numeric_.SetFloatValue(field, value);
        }

        public double DoubleValue(uint field)
        {
            return numeric_.DoubleValue(field);
        }

        public void SetDoubleValue(uint field, double value)
        {
            numeric_.SetDoubleValue(field, value);
        }

        public int IntValue(uint field)
        {
            return numeric_.IntValue(field);
        }

        public void SetIntValue(uint field, int value)
        {
            numeric_.SetIntValue(field, value);
        }

        public long LongValue(uint field)
        {
            return numeric_.LongValue(field);
        }

        public void SetLongValue(uint field, long value)
        {
            numeric_.SetLongValue(field, value);
        }
    }
}
