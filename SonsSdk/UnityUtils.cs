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
    
    /// <summary>
    /// Convert a world position to a position on a terrain texture in relation to a specified texture size.
    /// </summary>
    /// <param name="transformPosition"></param>
    /// <param name="terrain"></param>
    /// <param name="textureSize"></param>
    /// <returns></returns>
    public static Vector2? WorldPosToTextureSpace(Vector3 transformPosition, Terrain terrain = null, int textureSize = 2048)
    {
        if(!terrain)
            terrain = Terrain.activeTerrain;
        
        if (!terrain)
            return null;

        var position = terrain.GetPosition();
        var terrainData = terrain.terrainData;
        var posX = (transformPosition.x - position.x) / terrainData.size.x * textureSize;
        var posY = (transformPosition.z - position.z) / terrainData.size.z * textureSize;
        if (posX < 0 || posY < 0 || posX >= textureSize || posY >= textureSize)
        {
            return null;
        }

        return new(posX, posY);
    }
    
    /// <summary>
    /// Convert a position on a terrain texture to a world position in relation to a specified texture size.
    /// </summary>
    /// <param name="textureSpace"></param>
    /// <param name="terrain"></param>
    /// <param name="textureSize"></param>
    /// <returns></returns>
    public static Vector3 TextureSpaceToWorldPos(Vector2 textureSpace, Terrain terrain = null, int textureSize = 2048)
    {
        if(!terrain)
            terrain = Terrain.activeTerrain;
        
        if (terrain == null)
        {
            return Vector3.zero;
        }
        
        var position = terrain.GetPosition();
        var terrainData = terrain.terrainData;
        var x = textureSpace.x / textureSize * terrainData.size.x + position.x;
        var z = textureSpace.y / textureSize * terrainData.size.z + position.z;
        return new(x, terrain.SampleHeight(new Vector3(x, 0, z)) + position.y, z);
    }
}