using System.Reflection;
using Endnight.Extensions;
using ForestNanosuit;
using Il2CppSystem.Linq;
using RedLoader;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using Color = System.Drawing.Color;
using Object = UnityEngine.Object;

namespace SonsSdk;

/// <summary>
/// Utilities for addressables
/// </summary>
public static class AssetLoaders
{
    public static T LoadAsset<T>(string name) where T : Object
    {
        return Addressables.LoadAssetAsync<T>(name).WaitForCompletion();
    }

    /// <summary>
    /// Load a gameobject from addressables
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static GameObject LoadPrefab(string name)
    {
        return Addressables.LoadAssetAsync<GameObject>(name).WaitForCompletion();
    }

    /// <summary>
    /// Load and instantiate a gameobject from addressables
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static GameObject InstantiatePrefab(string name)
    {
        return Object.Instantiate(Addressables.LoadAssetAsync<GameObject>(name).WaitForCompletion());
    }
    
    /// <summary>
    /// Load an asset bundle from the calling assembly. The name will automatically be prefixed with the assembly name.
    /// </summary>
    /// <param name="name">The patch of the resource you wish to load</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    /// <example>LoadFromAssembly("Resources.bundle")</example>
    public static AssetBundle LoadBundleFromAssembly(string name)
    {
        return AssetBundle.LoadFromMemory(LoadDataFromAssembly(name));
    }
    
    /// <summary>
    /// Load data from the calling assembly. The name will automatically be prefixed with the assembly name.
    /// </summary>
    /// <param name="name">The patch of the resource you wish to load</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    /// <example>LoadFromAssembly("Resources.bundle")</example>
    public static byte[] LoadDataFromAssembly(string name)
    {
        var ass = Assembly.GetCallingAssembly();
        var ns = ass.GetName().Name;
        var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(ns + name);
        if(stream == null)
            throw new Exception("Failed to load data from assembly. Stream is null.");
        
        var bytes = new byte[stream.Length];
        _ = stream.Read(bytes, 0, bytes.Length);
        return bytes;
    }

    public static void PrintAllAddressables()
    {
        var writer = new UFileWriter("Addressables");

        var map = Addressables.ResourceLocators.First().Cast<ResourceLocationMap>();
        map.Keys.ForEach(new Action<Il2CppSystem.Object>(x =>
        {
            var str = x.Cast<Il2CppSystem.String>();

            try
            {
                var obj = Addressables.LoadAsset<Object>(str).WaitForCompletion();
                var objType = obj.GetIl2CppType().Name;
                writer.Add($"[{objType}] {obj.name} ");
            }
            catch (Exception e)
            { }

            //Extensions.Log(str);
            writer.Line($"<{str}>");
        }));

        writer.Save();

        RLog.Msg(Color.Chartreuse, "============== FINISHED WRITING ADDRESSABLES ==============");
    }
}