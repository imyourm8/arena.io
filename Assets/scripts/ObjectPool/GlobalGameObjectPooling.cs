using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ObjectPool 
{
	public class GlobalGameObjectPooling : SingletonMonobehaviour<GlobalGameObjectPooling>
	{
		private Dictionary<GameObject, IObjectPool<GameObject>> pools_;
		private Dictionary<GameObject, GameObject> objectsByPrefabs_;
	
		void Start()
		{
			if (Instance == null)
			{
				Instance = this;
				DontDestroyOnLoad(this);
				
				pools_ = new Dictionary<GameObject, IObjectPool<GameObject>>();
				objectsByPrefabs_ = new Dictionary<GameObject, GameObject>();
			} else 
			{
				Destroy(this);
			}
		}
		
		private IObjectPool<GameObject> GetPool(GameObject obj)
		{
			IObjectPool<GameObject> pool; 

			if (!pools_.TryGetValue(obj, out pool))
			{
				var newPool = new ObjectPoolPrefab(obj);
                newPool.ParentObj = gameObject;
                pool = newPool;
				pool.Init(20, 0, false);
				pools_.Add(obj, pool);
			}
			
			return pool;
		}
		
		public void Return(GameObject obj)
		{
			var prefab = objectsByPrefabs_ [obj];
			GetPool(prefab).Return(obj);
			objectsByPrefabs_.Remove (obj);
            gameObject.AddChild(obj);
		}
		
		public GameObject Get(GameObject obj)
		{
			var pool = GetPool (obj);
			var pooledObj = pool.Get ();
			objectsByPrefabs_[pooledObj] = obj;
			return pooledObj;
		}

        public void ReturnAll(GameObject obj)
        {
            GetPool(obj).Reset();
        }
	}
}

static public class GlobalGameObjectPoolingExtensions
{
	public static GameObject GetPooled(this GameObject obj)
	{
		return ObjectPool.GlobalGameObjectPooling.Instance.Get(obj);
	}
	
	public static void ReturnPooled(this GameObject obj)
	{
		ObjectPool.GlobalGameObjectPooling.Instance.Return(obj);
	}

    public static void ReturnPooledAll(this GameObject obj)
    {
        ObjectPool.GlobalGameObjectPooling.Instance.ReturnAll(obj);
    }
}