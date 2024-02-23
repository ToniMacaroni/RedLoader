using System.Diagnostics;
using RedLoader;
using RedLoader.Utils;
using TMPro;
using UnityEngine;

namespace SonsSdk;

public static class GameResources
{
    internal static Dictionary<string, Material> Materials = new();
    internal static Dictionary<string, Sprite> Sprites = new();
    internal static Dictionary<string, Texture2D> Textures = new();
    internal static Dictionary<string, Shader> Shaders = new();
    internal static Dictionary<string, TMP_FontAsset> Fonts = new();

    internal static void Load()
    {
        var sw = new TimingLogger(nameof(GameResources));
        
        Materials.Clear();
        Sprites.Clear();
        Textures.Clear();
        Shaders.Clear();
        Fonts.Clear();

        var materials = Resources.FindObjectsOfTypeAll<Material>();
        foreach (var material in materials)
        {
            Materials[material.name] = material;
        }
        
        var sprites = Resources.FindObjectsOfTypeAll<Sprite>();
        foreach (var sprite in sprites)
        {
            Sprites[sprite.name] = sprite;
        }
        
        var textures = Resources.FindObjectsOfTypeAll<Texture2D>();
        foreach (var texture in textures)
        {
            Textures[texture.name] = texture;
        }
        
        var shaders = Resources.FindObjectsOfTypeAll<Shader>();
        foreach (var shader in shaders)
        {
            Shaders[shader.name] = shader;
        }
        
        var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        foreach (var font in fonts)
        {
            Fonts[font.name] = font;
        }
        
        PrintAssets();
        //CreateMappings(new PathObject(@"I:\repos\MelonLoader\SonsSdk\AssetMaps"));
        
        sw.Stop("Loading resources");
    }

    [Conditional("DEBUG")]
    private static void CheckAsset(UnityEngine.Object obj, string name)
    {
        if (!obj)
        {
            RLog.Error($"Requested asset '{name}' is null!");
        }
    }
    
    public static Material GetMaterial(string name)
    {
        var asset = Materials.TryGetValue(name, out var mat) ? mat : null;
        CheckAsset(asset, name);
        return asset;
    }
    
    public static Sprite GetSprite(string name)
    {
        var asset = Sprites.TryGetValue(name, out var sprite) ? sprite : null;
        CheckAsset(asset, name);
        return asset;
    }
    
    public static Texture2D GetTexture(string name)
    {
        var asset = Textures.TryGetValue(name, out var texture) ? texture : null;
        CheckAsset(asset, name);
        return asset;
    }
    
    public static Shader GetShader(string name)
    {
        var asset = Shaders.TryGetValue(name, out var shader) ? shader : null;
        CheckAsset(asset, name);
        return asset;
    }
    
    public static TMP_FontAsset GetFont(string name)
    {
        var asset = Fonts.TryGetValue(name, out var font) ? font : null;
        CheckAsset(asset, name);
        return asset;
    }
    
    public static void PrintAssets()
    {
        void Log(string text)
        {
            RLog.Msg(System.Drawing.Color.Aqua, text);
        }

        PrettyPrint.Print("Materials", Materials.Keys, Log);
        PrettyPrint.Print("Sprites", Sprites.Keys, Log);
        PrettyPrint.Print("Textures", Textures.Keys, Log);
        PrettyPrint.Print("Shaders", Shaders.Keys, Log);
        PrettyPrint.Print("Fonts", Fonts.Keys, Log);
    }

    public static void CreateMappings(PathObject basePath)
    {
        AssetMapCreator.CreateAssetMap(basePath, "Material", Materials.Keys);
        AssetMapCreator.CreateAssetMap(basePath, "Sprite", Sprites.Keys);
        AssetMapCreator.CreateAssetMap(basePath, "Texture", Textures.Keys);
        AssetMapCreator.CreateAssetMap(basePath, "Shader", Shaders.Keys);
        AssetMapCreator.CreateAssetMap(basePath, "Font", Fonts.Keys);
    }
}