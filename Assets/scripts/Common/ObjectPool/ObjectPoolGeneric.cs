#define TRACK_ALL
#if UNITY_5
using UnityEngine;
#endif
using System;
using System.Collections;
using System.Collections.Generic;

namespace ObjectPool 
{
	public class ObjectPoolGeneric<T> : IObjectPool<T>
		where T : class, new()
	{
		private int nextIndex_ = 0;
		private List<T> pool_ = new List<T>();
		
		#if TRACK_ALL
		private HashSet<T> pooledObjects_ = new HashSet<T>();
		#endif
        #if UNITY_5
		[SerializeField]
		private int initialSize_ = 10;
		
		[SerializeField]
		private int maxSize_ = 10;
		
		[SerializeField]
		private bool fixedSize_ = false;
        #else
        private int initialSize_ = 10;
        private int maxSize_ = 10;
        private bool fixedSize_ = false;
        #endif

        public ObjectPoolGeneric(bool init = false)
        {
            if (init)
            {
                Init(1, 100, false);
            }
        }

		private void IncreasePoolSize(int addCount)
		{
			if (addCount < 1)
			{
				return;
			}
			for(var i = 0; i < addCount; ++i)
			{
				var obj = CreateObject();
				pool_.Add(obj);
			}
		}
		
		public void Init(int initialSize, int maxSize, bool isFixed)
		{
			initialSize_ = initialSize;
			maxSize_ = maxSize;
			fixedSize_ = isFixed;
			#if UNITY_5
			IncreasePoolSize (Mathf.Min(Mathf.Max (initialSize_, 1), 5000));
            #else       
            IncreasePoolSize(Math.Min(Math.Max(initialSize_, 1), 5000));
            #endif
            }
		
		public void Return(T obj)
		{
		#if TRACK_ALL
			if (!pooledObjects_.Contains(obj))
			{
            #if UNITY_5
				throw new UnityException("Object is not from pool");
            #else
                throw new Exception("Object is not from pool");
            #endif
			}
            pooledObjects_.Remove(obj);
		#endif
		
			OnObjectGetReturned(obj);
			pool_ [--nextIndex_] = obj;
		}

        public void Reset()
        {
            while(nextIndex_ > 0)
            {
                nextIndex_--;
                OnObjectGetReturned(pool_[nextIndex_]);
            }
        }

		public T Get()
		{
			if (nextIndex_ >= pool_.Count) 
			{
#if UNITY_5
			    int toAdd = Mathf.FloorToInt((float)pool_.Count * 1.6f);
				
				if (fixedSize_)
				{
					toAdd = Mathf.Min(maxSize_-pool_.Count, toAdd);
				}
#else
                int toAdd = (int)Math.Floor((float)pool_.Count * 1.6f);

                if (fixedSize_)
                {
                    toAdd = Math.Min(maxSize_ - pool_.Count, toAdd);
                }
#endif
				
				IncreasePoolSize(toAdd);
			}
			
			if (nextIndex_ >= pool_.Count)
			{
				return null;
			}
			
			T obj = pool_[nextIndex_++];
			OnObjectGetPoped(obj);
			
			#if TRACK_ALL
			pooledObjects_.Add(obj);
			#endif 
			
			return obj;
		}
		
		protected virtual void OnObjectGetPoped(T obj)
		{}
		
		protected virtual void OnObjectGetReturned(T obj)
		{}
		
		protected virtual T CreateObject()
		{
			return new T();
		}
	}
}