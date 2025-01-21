using System.Reflection;
using Il2CppInterop.Runtime;
using RedLoader;
using RedLoader.Utils;
using UnityEngine;
using Color = System.Drawing.Color;
using Object = UnityEngine.Object;

namespace SonsSdk.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class AssetBundleAttribute : Attribute
{
    public string AssetBundleName { get; }

    public AssetBundleAttribute(string assetBundleName)
    {
        AssetBundleName = assetBundleName;
    }

    public bool IsLoaded()
    {
        return (bool)LoadedBundle;
    }

    internal bool LoadBundle(Assembly assembly)
    {
        if (IsLoaded())
        {
            return true;
        }

        var absolutePath = LoaderEnvironment.GetModDataPath(assembly) / AssetBundleName;
        if (!absolutePath.FileExists())
        {
            RLog.Error($"Couldn't find asset bundle {absolutePath.Path}");
            return false;
        }

        LoadedBundle = AssetBundle.LoadFromFile(absolutePath.Path);
        if (!LoadedBundle)
        {
            RLog.Error($"Couldn't load asset bundle {absolutePath.Path}");
            return false;
        }

        foreach (var assetReference in AssetReferences)
        {
            assetReference.LoadAsset();
        }
        
        RLog.Msg(Color.Orange, $"Loaded bundle {AssetBundleName} with {AssetReferences.Count} assets");

        return true;
    }

    internal AssetBundle LoadedBundle;

    internal List<AssetReferenceAttribute> AssetReferences = new();
}

[AttributeUsage(AttributeTargets.Property)]
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
            RLog.Error($"Asset bundle {AssetBundle.AssetBundleName} is not loaded!");
            return;
        }

        if (!Property.CanWrite)
        {
            RLog.Error($"Property {Property.Name} is not writable!");
            return;
        }

        if (string.IsNullOrEmpty(AssetName))
        {
            AssetName = Property.Name;
        }

        var asset = AssetBundle.LoadedBundle.LoadAsset(AssetName, Il2CppType.From(Property.PropertyType));
        asset.hideFlags = HideFlags.HideAndDontSave;
        
        if (!asset)
        {
            RLog.Error($"Couldn't find asset {AssetName} in bundle {AssetBundle.AssetBundleName}");
            return;
        }

        var actualType = Property.PropertyType == typeof(Object)
            ? asset
            : DynamicInitializerStore.GetInitializer(Property.PropertyType)(asset.Pointer);
        
        Property.SetValue(ClassInstance, actualType);
    }
}

internal static class AssetBundleAttributeLoader
{
    public static List<AssetBundleAttribute> GetAllTypes(ModBase mod)
    {
        var melonAssembly = mod.ModAssembly;

        var assetBundles = new List<AssetBundleAttribute>();

        foreach (var type in melonAssembly.GetTypes())
        {
            var assetBundleAttribute = type.GetCustomAttribute<AssetBundleAttribute>();
            if (assetBundleAttribute == null)
            {
                continue;
            }

            foreach (var prop in type.GetProperties())
            {
                var assetReferenceAttribute = prop.GetCustomAttribute<AssetReferenceAttribute>();
                if (assetReferenceAttribute == null)
                {
                    continue;
                }

                assetReferenceAttribute.AssetBundle = assetBundleAttribute;
                assetReferenceAttribute.Property = prop;
                assetReferenceAttribute.ClassInstance = null;

                assetBundleAttribute.AssetReferences.Add(assetReferenceAttribute);
            }

            assetBundles.Add(assetBundleAttribute);
        }

        return assetBundles;
    }
}
