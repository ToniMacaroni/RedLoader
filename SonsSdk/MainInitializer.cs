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
using SonsSdk.AssetImporting;
using SonsSdk.Building;
using SonsSdk.Networking;
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
        
        GameResources.Load();

        SceneManager.LoadScene(SonsSceneManager.OptionsMenuSceneName, LoadSceneMode.Additive);
        yield return null;
        SUI.SUI.InitPrefabs();
        SceneManager.UnloadScene(SonsSceneManager.OptionsMenuSceneName);

        yield return loadCatalogsTask;

        InitSystems();
        
        SdkEvents.OnSdkInitialized.Invoke();
    }

    // Gets called either through InitCoro or through SonsGameManager when it's a dedicated server
    internal static void InitSystems()
    {
        if (!LoaderEnvironment.IsDedicatedServer)
        {
            GlobalInput.Init();
            GenericModalDialog.Setup();
            TooltipProvider.Setup();
            SonsUiTools.Init();
        }

        SonsSaveTools.Init();
        GameManagers.Init();
        Packets.Init();
        EntityManager.Init();
        AssetImportingInitializer.Init();
        CraftingNodeCreator.Init();
    }

    private static void LoadAllModBundles()
    {
        foreach (var mod in SonsMod.RegisteredMods)
        {
            foreach (var assetBundle in mod.AssetBundleAttrs)
            {
                assetBundle.LoadBundle(mod.ModAssembly);
            }
        }
    }

    private static bool _isInitialized;
}
