using System.Reflection;
using Il2CppInterop.Runtime;
using RedLoader;
using RedLoader.Utils;
using UnityEngine;

namespace SonsSdk.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class AssetBundleAttribute : Attribute
{
    public string AssetBundleName { get; }

    internal AssetBundle LoadedBundle;

    public AssetBundleAttribute(string assetBundleName)
    {
        AssetBundleName = assetBundleName;
    }

    internal bool LoadBundle(MelonAssembly melonAssembly)
    {
        if (IsLoaded())
            return true;
        
        var absolutePath = MelonEnvironment.GetModDataPath(melonAssembly.Assembly) / AssetBundleName;
        if (!absolutePath.FileExists())
        {
            MelonLogger.Error($"Couldn't find asset bundle {absolutePath.Path}");
            return false;
        }
        
        LoadedBundle = AssetBundle.LoadFromFile(absolutePath.Path);
        if (!LoadedBundle)
        {
            MelonLogger.Error($"Couldn't load asset bundle {absolutePath.Path}");
            return false;
        }

        return true;
    }

    public bool IsLoaded()
    {
        return (bool)LoadedBundle;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class AssetReferenceAttribute : Attribute
{
    public string AssetName { get; private set; }
    
    internal PropertyInfo Property { get; set; }
    internal object ClassInstance { get; set; }

    internal AssetBundleAttribute AssetBundle { get; set; }
    
    public AssetReferenceAttribute(string assetName = null)
    {
        AssetName = assetName;
    }
    
    internal void LoadAsset()
    {
        if (!AssetBundle.IsLoaded())
        {
            MelonLogger.Error($"Asset bundle {AssetBundle.AssetBundleName} is not loaded!");
            return;
        }

        if (!Property.CanWrite)
        {
            MelonLogger.Error($"Property {Property.Name} is not writable!");
            return;
        }

        if (string.IsNullOrEmpty(AssetName))
            AssetName = Property.Name;

        var asset = AssetBundle.LoadedBundle.LoadAsset(AssetName, Il2CppType.From(Property.PropertyType));
        if (!asset)
        {
            MelonLogger.Error($"Couldn't find asset {AssetName} in bundle {AssetBundle.AssetBundleName}");
            return;
        }

        Property.SetValue(ClassInstance, asset);
    }
}

internal static class AssetBundleAttributeLoader
{
    public static void Load(MelonBase mod)
    {
        var melonAssembly = mod.MelonAssembly;
        
        foreach (var type in melonAssembly.Assembly.GetTypes())
        {
            var assetBundleAttribute = type.GetCustomAttribute<AssetBundleAttribute>();
            if (assetBundleAttribute == null)
                continue;

            MelonLogger.Msg("Found asset bundle attribute " + assetBundleAttribute.AssetBundleName);
            assetBundleAttribute.LoadBundle(melonAssembly);
            
            foreach (var prop in type.GetProperties())
            {
                var assetReferenceAttribute = prop.GetCustomAttribute<AssetReferenceAttribute>();
                if (assetReferenceAttribute == null)
                    continue;
                
                MelonLogger.Msg("Found asset reference attribute " + assetReferenceAttribute.AssetName);

                assetReferenceAttribute.AssetBundle = assetBundleAttribute;
                assetReferenceAttribute.Property = prop;
                assetReferenceAttribute.ClassInstance = null;
                
                assetReferenceAttribute.LoadAsset();
            }
        }
    }
}