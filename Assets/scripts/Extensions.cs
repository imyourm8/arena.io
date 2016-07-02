using UnityEngine;
using System;
//using System.Linq;
using System.Collections.Generic;
using System.Text;

public static class Extensions
{
    public static void ForEachValue<T,V>(this Dictionary<T,V> dict, Action<V> cb)
    {
        foreach(var pair in dict)
        {
            cb(pair.Value);
        }
    }
    
    public static void Resize<T>(this List<T> list, int capacity)
    {
        if (list.Capacity < capacity)
            list.Capacity = capacity;
    }

    static StringBuilder builder = new StringBuilder();
    public static string ToHex (this Color color) 
    {
        builder.Remove(0, builder.Length);
        builder.Append("#");
        
        int r = Mathf.FloorToInt(color.r * 255.0f);
        int g = Mathf.FloorToInt(color.g * 255.0f);
        int b = Mathf.FloorToInt(color.b * 255.0f);
        int a = Mathf.FloorToInt(color.a * 255.0f);
        
        builder.Append(r.ToString("X2"));
        builder.Append(g.ToString("X2"));
        builder.Append(b.ToString("X2"));
        builder.Append(a.ToString("X2"));
        
        return builder.ToString();
    }

    public static void AddChild(this GameObject obj, GameObject child)
    {
        child.transform.SetParent(obj.transform, false); 
    }
    
    public static void Detach(this GameObject obj)
    {
        obj.transform.SetParent(null, false);
    }
    
    public static void Destroy(this GameObject obj)
    {
        GameObject.Destroy(obj);
    }

    public static GameObject GetParent(this GameObject obj)
    {
        return obj.transform.parent == null ? null : obj.transform.parent.gameObject;
    }
    /*
    public static void DetachChildren(this GameObject obj)
    {
        List<GameObject> toDelete = ListPool<GameObject>.Get();
        obj.Map((GameObject ch)=>
        {
            toDelete.Add(ch);
        });
        
        toDelete.ForEach((o)=>
        {
            o.Detach();
        });
        
        ListPool<GameObject>.Release(toDelete);
    }
    
    public static void Map(this GameObject obj, Action<GameObject> functor)
    {   
        List<GameObject> toMap = ListPool<GameObject>.Get();
        var tran = obj.transform;
        int count = tran.childCount;
        for(var i = 0; i < count; ++i)
        {
            toMap.Add(tran.GetChild(i).gameObject);
        }
        for (var i = 0; i < count; ++i)
            functor(toMap[i]);
        ListPool<GameObject>.Release(toMap);
    }
    
    public static void DestroyChildren(this GameObject obj)
    {
        List<GameObject> toDelete = ListPool<GameObject>.Get();
        foreach(Transform ch in obj.transform)
        {
            toDelete.Add(ch.gameObject);
        }
        
        toDelete.ForEach((o)=>
        {
            o.Detach();
            GameObject.Destroy(o);
        });
        
        ListPool<GameObject>.Release(toDelete);
    }

    // Initially SortChildren method had this signature: public static void SortChildren<TOrderBy>(this GameObject obj, Func<GameObject, TOrderBy> orderBy)
    // and used this for sorting: var sorted = children.OrderBy(orderBy);
    // However OrderBy on iOS throws and exception "Attempting to JIT compile method while running with --aot-only."
    // so implemented sorting via list sort with comparison delegate
    public static void SortChildren(this GameObject obj, Comparison<GameObject> comparison)
    {   
        var children = ListPool<GameObject>.Get();
        
        obj.Map((GameObject child)=>
        {
            children.Add(child);
        });
        
        children.Sort(comparison);
        for (int i = 0; i < children.Count; i++)
        {
            children[i].transform.SetSiblingIndex(i);
        }
        ListPool<GameObject>.Release(children);
    }*/
    
    public static void SetActiveChildren(this GameObject obj, bool value)
    {
        foreach(Transform ch in obj.transform)
        {
            ch.gameObject.SetActive(value);
        }
    }
    
    public static GameObject GetChildAt(this GameObject obj, int index)
    {
        return obj.transform.GetChild(index).gameObject;
    }
}
   