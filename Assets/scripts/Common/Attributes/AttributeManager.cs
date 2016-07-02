using System.Collections.Generic;

namespace Attributes
{
    public class AttributeManager<T>
    {
        private Dictionary<T, Attribute<T>> attributes_;

        public AttributeManager()
        {
            attributes_ = new Dictionary<T, Attribute<T>>();
        }

        public void Add(Attribute<T> att)
        {
            if (attributes_.ContainsKey(att.ID))
                return;
            attributes_.Add(att.ID, att);
        }

        public Attribute<T> Get(T id)
        {
            Attribute<T> att = null;
            attributes_.TryGetValue(id, out att);
            return att;
        }

        public float GetFinValue(T id)
        {
            float value = 0.0f;
            Attribute<T> att = Get(id);
            if (att != null)
            {
                value = att.FinalValue;
            }
            return value;
        }

        public void SetValue(T id, float value)
        {
            Attribute<T> att = Get(id);
            if (att != null)
            {
                att.SetValue(value);
            }
        }

        public IEnumerator<Attribute<T>> GetEnumerator()
        {
            return attributes_.Values.GetEnumerator();
        }
    }
}