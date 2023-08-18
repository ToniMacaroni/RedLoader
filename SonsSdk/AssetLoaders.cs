using System;
using System.Collections;
using Endnight.Extensions;
using Endnight.Utilities;
using Il2CppSystem.Linq;
using SFLoader;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using CollectionExtensions = BepInEx.Unity.IL2CPP.Utils.Collections.CollectionExtensions;
using Color = System.Drawing.Color;
using Object = UnityEngine.Object;

namespace ForestNanosuit;

public static class AssetLoaders
{
    public static ResourceLocationMap ResourceLocator { get; private set; }

    public static void Initialize()
    {
        CoroutineHelper.StartCoroutine(CollectionExtensions.WrapToIl2Cpp(InitializeAsync()));
    }

    public static IEnumerator InitializeAsync()
    {
        var resourceLocator = Addressables.LoadContentCatalogAsync(
            @"F:\ForestNanosuit\Library\com.unity.addressables\aa\Windows\StandaloneWindows64\catalog_0.1.0.json");
        yield return resourceLocator;

        ResourceLocator = resourceLocator.Result.Cast<ResourceLocationMap>();

        yield return Addressables.InitializeAsync();
    }

    public static T LoadAsset<T>(string name) where T : Object
    {
        return Addressables.LoadAssetAsync<T>(name).WaitForCompletion();
    }

    public static GameObject LoadPrefab(string name)
    {
        return Addressables.LoadAssetAsync<GameObject>(name).WaitForCompletion();
    }

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

        MelonLogger.Msg(Color.Chartreuse, "============== FINISHED WRITING ADDRESSABLES ==============");
    }
}