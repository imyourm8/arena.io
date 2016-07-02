#define TRACK_ALL

using UnityEngine;
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
		
		[SerializeField]
		private int initialSize_ = 10;
		
		[SerializeField]
		private int maxSize_ = 10;
		
		[SerializeField]
		private bool fixedSize_ = false;

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
			
			IncreasePoolSize (Mathf.Min(Mathf.Max (initialSize_, 1), 5000));
		}
		
		public void Return(T obj)
		{
		#if TRACK_ALL
			if (!pooledObjects_.Contains(obj))
			{
				throw new UnityException("Object is not from pool");
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
				int toAdd = Mathf.FloorToInt((float)pool_.Count * 1.6f);
				
				if (fixedSize_)
				{
					toAdd = Mathf.Min(maxSize_-pool_.Count, toAdd);
				}
				
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