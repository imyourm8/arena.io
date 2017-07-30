#if UNITY
using UnityEngine;
#endif

namespace shared
{
    public class Singleton<T> where T : class, new()
    {
        class SingletonCreator
        {
            static SingletonCreator() { }
            internal static readonly T instance_ = new T();
        }

        protected static T instance_;

        public static T Instance
        {
            get { return SingletonCreator.instance_; }
			protected set { instance_ = value; }
        }
    }

    public class SingletonOnDemand<T> where T : class, new()
    {
        protected static T instance_;

        public static T Instance
        {
            get { return instance_; }
            protected set { instance_ = value; }
        }
    }

#if UNITY
    public class SingletonBehaviour<ChildType> : MonoBehaviour
        where ChildType : SingletonBehaviour<ChildType>
    {
        #region Properties

        public static ChildType Instance { get; protected set; }

        public static bool HasInstance { get { return Instance != null; } }

        #endregion


        #region Unity Lifecycle

        protected virtual void Awake()
        {
            if (Instance != null)
            {
                return;
            }
            Instance = this as ChildType;
        }

        protected virtual void OnDestroy()
        {
            Instance = null;
        }

        #endregion
    }

#endif
}
