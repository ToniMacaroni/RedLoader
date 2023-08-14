using System.Diagnostics;
using UnityEngine;
using Color = System.Drawing.Color;
using Object = UnityEngine.Object;

namespace SonsSdk;

public static class CommonExtensions
{
    public static GameObject Instantiate(this GameObject go, bool sameParent = false)
    {
        return Object.Instantiate(go, sameParent ? go.transform.parent : null);
    }
    
    public static T FindGet<T>(this GameObject go, string name) where T : Component
    {
        return go.transform.Find(name).GetComponent<T>();
    }

    public static GameObject DontDestroyOnLoad(this GameObject go)
    {
        Object.DontDestroyOnLoad(go);
        return go;
    }

    public static GameObject HideAndDontSave(this GameObject go)
    {
        go.hideFlags = HideFlags.HideAndDontSave;
        return go;
    }

    public static GameObject SetName(this GameObject go, string name)
    {
        go.name = name;
        return go;
    }
    
    public static GameObject SetParent(this GameObject go, Transform parent, bool worldPositionStays = false)
    {
        go.transform.SetParent(parent, worldPositionStays);
        return go;
    }

    public static void Destroy<T>(this GameObject go) where T : Component
    {
        var component = go.GetComponent<T>();
        
        if (component)
        {
            Object.Destroy(component);
        }
    }
    
    public static UnityEngine.Color ToUnityColor(this Color color)
    {
        return new UnityEngine.Color(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
    }
    
    public static UnityEngine.Color WithAlpha(this UnityEngine.Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }
    
    public static T FirstWithName<T>(this IEnumerable<T> iter, string name) where T : Object
    {
        return iter.First(x => x.name == name);
    }

    public static T FirstStartsWith<T>(this IEnumerable<T> iter, string name) where T : Object
    {
        return iter.First(x => x.name.StartsWith(name));
    }

    public static T FirstEndsWith<T>(this IEnumerable<T> iter, string name) where T : Object
    {
        return iter.First(x => x.name.EndsWith(name));
    }

    public static T FirstContains<T>(this IEnumerable<T> iter, string name) where T : Object
    {
        return iter.First(x => x.name.Contains(name));
    }
}