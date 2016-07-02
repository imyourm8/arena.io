using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TapCommon
{
    abstract public class SingletonAbstractFactory<KeyType, ClassType, DerivedClass> :
        Singleton<DerivedClass> where ClassType : class where DerivedClass : class, new()
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
            Debug.Assert(obj != null, "No Creator found");
            return obj;
        }

        public void RegisterType<Class>(KeyType key) where Class : ClassType, new()
        {
            creators_.Add(key, new ConcreateCreator<Class>());
        }

        abstract public void Init();
    }
}
