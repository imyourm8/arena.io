using System.Collections;

namespace ObjectPool 
{
	public interface IObjectPool<T>
	{
		T Get();
        void Reset();
		void Return(T obj);
		void Init(int initialSize, int maxSize, bool isFixed);
	}
}