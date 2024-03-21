using UnityEngine;

namespace SonsSdk;

public static class UnityUtils
{
    /// <summary>
    /// Convert a texture to a sprite. This is a slow operation. Make sure to cache the result.
    /// </summary>
    /// <param name="tex"></param>
    /// <returns></returns>
    public static Sprite ToSprite(this Texture2D tex)
    {
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
    }
    
    /// <summary>
    /// Get a component or adds it to the gameobject if it doesn't exist
    /// </summary>
    /// <param name="go"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T AddOrGet<T>(this GameObject go) where T : Component
    {
        return go.TryGetComponent<T>(out var comp) ? comp : go.AddComponent<T>();
    }
}