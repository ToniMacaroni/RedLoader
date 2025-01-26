using Alt.Json;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using RedLoader;
using SonsSdk.JsonConverters;
using UnityEngine;
using Color = System.Drawing.Color;

namespace SonsSdk;

public static class UnityUtils
{
    private static JsonSerializerSettings _jsonSerializerSettings;
    public static JsonSerializerSettings JsonSerializerSettings
    {
        get
        {
            if (_jsonSerializerSettings == null)
            {
                _jsonSerializerSettings = new JsonSerializerSettings();
                _jsonSerializerSettings.Converters.Add(new Vec2Converter());
                _jsonSerializerSettings.Converters.Add(new Vec3Converter());
                _jsonSerializerSettings.Converters.Add(new Vec4Converter());
                _jsonSerializerSettings.Converters.Add(new ColorConverter());
                _jsonSerializerSettings.Converters.Add(new QuaternionConverter());
            }

            return _jsonSerializerSettings;
        }
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
    
    /// <summary>
    /// Prints a <see cref="Transform"/> hierarchy. Useful for debugging on the server where no GUI is available.
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="level"></param>
    public static void PrintHierarchy(Transform transform, int level = 0)
    {
        if (transform == null)
            return;

        var typesString = " (" + string.Join(", ", transform.GetComponents<Component>().Select(x => x.GetIl2CppType().Name)) + ")";

        RLog.Msg(level == 0 ? System.Drawing.Color.Aquamarine : System.Drawing.Color.CadetBlue, $"{new string('-', level)}{transform.name}");
        RLog.Msg(System.Drawing.Color.Orange, $"{new string(' ', level)}{typesString}");
        foreach (var child in transform.GetChildren())
        {
            PrintHierarchy(child, level + 1);
        }
    }

    public static void AsPrefab(this GameObject go, params MonoBehaviour[] compsToEnable)
    {
        go.DontDestroyOnLoad();
        var enabler = go.AddComponent<PrefabComponentEnabler>();
        enabler.ComponentsToEnable.Set(compsToEnable.ToIl2CppList());
    }
    
    public class PrefabComponentEnabler : MonoBehaviour
    {
        public Il2CppReferenceField<Il2CppSystem.Collections.Generic.List<MonoBehaviour>> ComponentsToEnable;

        static PrefabComponentEnabler()
        {
            ClassInjector.RegisterTypeInIl2Cpp<PrefabComponentEnabler>();
        }

        public void OnEnable()
        {
            var compList = ComponentsToEnable.Get();
            if (compList == null)
            {
                return;
            }
            
            if (gameObject.scene.name != "DontDestroyOnLoad")
            {
                foreach (var comp in compList)
                {
                    comp.enabled = true;
                }
                Destroy(this);
            }
        }
    }

    public static string JsonSerialize<T>(T obj)
    {
        return JsonConvert.SerializeObject(obj, JsonSerializerSettings);
    }

    public static T JsonDeserialize<T>(string data)
    {
        return JsonConvert.DeserializeObject<T>(data, JsonSerializerSettings);
    }
}
