using UnityEngine;

namespace SonsSdk;

public static class UnityUtils
{
    /// <summary>
    /// Load a texture from a byte buffer
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static Texture2D LoadTexture(byte[] data)
    {
        var tex = new Texture2D(2, 2);
        tex.LoadImage(data);
        return tex;
    }
    
    /// <summary>
    /// Load a texture from a file
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Texture2D LoadTexture(string path)
    {
        return LoadTexture(File.ReadAllBytes(path));
    }
    
    /// <summary>
    /// Load a texture from a file in the calling assembly. The name will automatically be prefixed with the assembly name.
    /// </summary>
    /// <param name="path"></param>
    /// <example>LoadTextureFromAssembly("Resources.MyTexture.png")</example>
    /// <returns></returns>
    public static Texture2D LoadTextureFromAssembly(string path)
    {
        return LoadTexture(AssetLoaders.LoadDataFromAssembly(path));
    }
    
    /// <summary>
    /// Convert a texture to a sprite. This is a slow operation. Make sure to cache the result.
    /// </summary>
    /// <param name="tex"></param>
    /// <returns></returns>
    public static Sprite ToSprite(this Texture2D tex)
    {
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
    }
}