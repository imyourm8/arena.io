using UnityEngine;
using System.Collections.Generic;

public sealed class ListPool<T>
{
    private static readonly ObjectPool.ObjectPoolGeneric<List<T>> s_ListPool = new ObjectPool.ObjectPoolGeneric<List<T>>(true);

    public static List<T> Get(int preallocate = 10)
    {
        var list = s_ListPool.Get();
        if (list.Capacity < preallocate)
            list.Capacity = preallocate;
        return list;
    }
    
    public static void Release(List<T> toRelease)
    {
        if (toRelease == null) return;
        toRelease.Clear();
        s_ListPool.Return(toRelease);
    }
}
