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
    
    public static GameObject Instantiate(this GameObject go, Vector3 position, bool sameParent = false)
    {
        var ret = Object.Instantiate(go, sameParent ? go.transform.parent : null);
        ret.transform.position = position;
        return ret;
    }
    
    public static GameObject Instantiate(this GameObject go, Vector3 position, Quaternion rotation, bool sameParent = false)
    {
        var ret = Object.Instantiate(go, sameParent ? go.transform.parent : null);
        var t = ret.transform;
        t.position = position;
        t.rotation = rotation;
        return ret;
    }
    
    public static T InstantiateAndGet<T>(this GameObject go, bool sameParent = false) where T : Component
    {
        return go.Instantiate(sameParent).GetComponent<T>();
    }
    
    public static T InstantiateAndGet<T>(this GameObject go, Vector3 position, bool sameParent = false) where T : Component
    {
        return go.Instantiate(position, sameParent).GetComponent<T>();
    }
    
    public static T InstantiateAndGet<T>(this GameObject go, Vector3 position, Quaternion rotation, bool sameParent = false) where T : Component
    {
        return go.Instantiate(position, rotation, sameParent).GetComponent<T>();
    }

    /// <summary>
    /// Gets a transform by path and return a component on it
    /// </summary>
    /// <param name="go"></param>
    /// <param name="name">The path of the transform to get</param>
    /// <typeparam name="T">The type of the component to get</typeparam>
    /// <returns></returns>
    public static T FindGet<T>(this GameObject go, string name) where T : Component
    {
        return go.transform.Find(name).GetComponent<T>();
    }
    
    /// <summary>
    /// Gets a transform by path and return a component on it
    /// </summary>
    /// <param name="tr"></param>
    /// <param name="name">The path of the transform to get</param>
    /// <typeparam name="T">The type of the component to get</typeparam>
    /// <returns></returns>
    public static T FindGet<T>(this Transform tr, string name) where T : Component
    {
        return tr.Find(name).GetComponent<T>();
    }
    
    public static List<Transform> GetChildren(this Transform tr)
    {
        var ret = new List<Transform>();
        for(var i = 0; i < tr.childCount; i++)
        {
            ret.Add(tr.GetChild(i));
        }
        
        return ret;
    }
    
    public static List<Transform> GetChildren(this GameObject go)
    {
        return go.transform.GetChildren();
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

    public static void TryDestroy(this GameObject go)
    {
        if (!go)
            return;
        
        Object.Destroy(go);
    }
    
    public static void AddGo(this GameObject go, string name = "GameObject")
    {
        new GameObject(name).SetParent(go.transform);
    }
    
    public static T AddChildComp<T>(this GameObject go, string name = "GameObject") where T : Component
    {
        return new GameObject(name).SetParent(go.transform).AddComponent<T>();
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
    
    public static UnityEngine.Color WithBrightness(this UnityEngine.Color color, float brightness)
    {
        float h, s, v;
        UnityEngine.Color.RGBToHSV(color, out h, out s, out v);
        v = brightness;
        return UnityEngine.Color.HSVToRGB(h, s, v);
    }
    
    public static UnityEngine.Color WithBrightnessOffset(this UnityEngine.Color color, float brightnessOffset)
    {
        float h, s, v;
        UnityEngine.Color.RGBToHSV(color, out h, out s, out v);
        v += brightnessOffset;
        return UnityEngine.Color.HSVToRGB(h, s, v);
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
    
    public static T GetRandomEntry<T>(this List<T> lst)
    {
        return lst[UnityEngine.Random.Range(0, lst.Count)];
    }

    public static void Finish(this CancellationTokenSource cts)
    {
        cts.Cancel();
        cts.Dispose();
    }
    
    public static CancellationTokenSource ResetAndCreate(this CancellationTokenSource cts)
    {
        cts.Cancel();
        cts.Dispose();
        return new CancellationTokenSource();
    }
}