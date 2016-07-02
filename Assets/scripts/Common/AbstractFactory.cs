using System.Collections.Generic;

namespace TapCommon
{
    public class AbstractFactory<KeyType, ClassType> where ClassType : class
    {
        internal abstract class Creator
        {
            abstract public ClassType Create();
        };

        internal class ConcreateCreator<Class> : Creator where Class : ClassType, new()
        {
            override public ClassType Create()
            {
                return new Class();
            }
        };

        private Dictionary<KeyType, Creator> creators_ = new Dictionary<KeyType, Creator>();

        public ClassType Create(KeyType type)
        {
            Creator realization = null;
            ClassType obj = null;
            if (creators_.TryGetValue(type, out realization))
            {
                obj = realization.Create();
            }
            return obj;
        }

        public void RegisterType<Class>(KeyType key) where Class : ClassType, new()
        {
            creators_.Add(key, new ConcreateCreator<Class>());
        }
    }
}
