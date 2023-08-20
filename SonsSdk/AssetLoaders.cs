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