using System.Collections;
using System.Drawing;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.Runtime;
using RedLoader;
using RedLoader.Utils;
using Sons.Events;
using Sons.Input;
using Sons.Loading;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Color = System.Drawing.Color;

namespace SonsSdk;

internal class MainInitializer
{
    internal static void InitTitleScene()
    {
        if (_isInitialized)
        {
            return;
        }
        _isInitialized = true;

        InitCoro().RunCoro();
    }

    private static IEnumerator LoadAllCatalogs()
    {
        foreach (var mod in SonsMod.RegisteredMods)
        {
            var modDataFolder = LoaderEnvironment.GetModDataPath(mod);
            foreach (var catalog in modDataFolder.GetFiles("catalog*.json"))
            {
                RLog.Msg(Color.Orange, "Loading addressables catalog " + catalog);
                yield return Addressables.LoadContentCatalogAsync(catalog);
            }
        }
    }

    private static IEnumerator InitCoro()
    {
        LoadAllModBundles();
        var loadCatalogsTask = LoadAllCatalogs();

        SceneManager.LoadScene(SonsSceneManager.OptionsMenuSceneName, LoadSceneMode.Additive);
        yield return null;
        SUI.SUI.InitPrefabs();
        SceneManager.UnloadScene(SonsSceneManager.OptionsMenuSceneName);

        yield return loadCatalogsTask;

        SdkEvents.OnSdkInitialized.Invoke();
    }

    private static void LoadAllModBundles()
    {
        foreach (var mod in SonsMod.RegisteredMods)
        {
            foreach (var assetBundle in mod.AssetBundleAttrs)
            {
                assetBundle.LoadBundle(mod.MelonAssembly);
            }
        }
    }

    private static bool _isInitialized;
}