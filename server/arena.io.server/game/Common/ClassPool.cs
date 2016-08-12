using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.Common
{
    public sealed class ClassPool<T> where T : class, new()
    {
        private static readonly ObjectPool.ObjectPoolGeneric<T> pool_ = new ObjectPool.ObjectPoolGeneric<T>(true);

        public static T Get()
        {
            return pool_.Get();
        }

        public static void Release(T toRelease)
        {
            if (toRelease == null) return;
            pool_.Return(toRelease);
        }
    }
}
