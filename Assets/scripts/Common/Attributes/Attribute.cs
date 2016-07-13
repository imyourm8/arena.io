using System.Collections.Generic;

namespace Attributes 
{
    public class Attribute<T> : BaseAttribute<T>
    {
        private float rawValue_;
        private float finalValue_;
        protected List<Attribute<T>> attributes_;
        protected int steps_;
        protected float step_;

        public Attribute()
        {
            attributes_ = new List<Attribute<T>>();
            finalValue_ = 0.0f;
        }

        public Attribute<T> Init(T id, float value = 0.0f, float multiplier = 0.0f, float percentValue = 0.0f)
        {
            id_ = id;
            value_ = value;
            multiplier_ = multiplier;
            percentValue_ = percentValue;
            CalculateValue ();
            return this;
        }

        public void CopyFrom(Attribute<T> attribute)
        {
            finalValue_ = attribute.finalValue_;
            value_ = attribute.value_;
            percentValue_ = attribute.percentValue_;
            multiplier_ = attribute.multiplier_;
            CalculateValue ();
        }

        public void AddAttribute(Attribute<T> attribute)
        {
            attributes_.Add (attribute);
            CalculateValue ();
        }

        public void RemoveAttribute(Attribute<T> attribute)
        {
            attributes_.Remove (attribute);
            CalculateValue ();
        }

        public override void CalculateValue ()
        {
            finalValue_ = (GetRawValue() + GetValue() * (1.0f + GetMultiplier())) * (1.0f + GetPercentValue());
        }

        public float FinalValue
        {
            get { return finalValue_; }
        }

        public float RawValue
        {
            get { return rawValue_;  }
            set { rawValue_ = value; }
        }

        protected virtual float GetRawValue()
        {
            float value = rawValue_;
            int count = attributes_.Count;
            for(int i = 0; i < count; ++i)
            {
                value += attributes_[i].rawValue_;
            }
            
            return value;
        }

        protected virtual float GetValue()
        {
            float value = value_;
            int count = attributes_.Count;
            for(int i = 0; i < count; ++i)
            {
                value += attributes_[i].value_;
            }
            
            return value;
        }

        protected virtual float GetMultiplier()
        {
            float value = multiplier_;
            int count = attributes_.Count;
            for(int i = 0; i < count; ++i)
            {
                value += attributes_[i].multiplier_;
            }
            
            return value;
        }

        protected virtual float GetPercentValue()
        {
            float value = percentValue_;
            int count = attributes_.Count;
            for(int i = 0; i < count; ++i)
            {
                value += attributes_[i].percentValue_;
            }

            return value;
        }

        public Attribute<T> SetStep(float s)
        {
            step_ = s;
            return this;
        }

        public int Steps
        {
            get { return steps_; }
        }

        public void ResetSteps()
        {
            steps_ = 0;
        }

        public void IncreaseByStep()
        {
            rawValue_ += step_;
            steps_++;
            CalculateValue();
        }
    }
}