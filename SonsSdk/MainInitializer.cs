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
using SonsSdk.Private;
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

    private static IEnumerator AddProgress(int index, int total, float totalWorkload)
    {
        GlobalOverlays.ProgressBar.AddProgress(index/(float)total*totalWorkload);
        yield return null;
    }

    private static IEnumerator AddProgress(float amount)
    {
        GlobalOverlays.ProgressBar.AddProgress(amount);
        yield return null;
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
        yield return LoadAllModBundles();
        var loadCatalogsTask = LoadAllCatalogs();
        
        GameResources.Load();

        SceneManager.LoadScene(SonsSceneManager.OptionsMenuSceneName, LoadSceneMode.Additive);
        yield return null;
        SUI.SUI.InitPrefabs();
        SceneManager.UnloadScene(SonsSceneManager.OptionsMenuSceneName);
        
        yield return AddProgress(0.15f);

        yield return loadCatalogsTask;

        InitSystems();
        
        yield return AddProgress(0.15f);

        var subs = SdkEvents.OnSdkInitialized.GetSubscribers();
        for (var i = 0; i < subs.Length; i++)
        {
            subs[i].del?.Invoke();
            yield return AddProgress(i, subs.Length, 0.4f);
        }

        SdkEvents.OnSdkLateInitialized.Invoke();
        GlobalOverlays.Hide();
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

    private static IEnumerator LoadAllModBundles()
    {
        var assemblies = SonsMod.RegisteredMods.SelectMany(x => x.AssetBundleAttrs).ToArray();

        for (var i = 0; i < assemblies.Length; i++)
        {
            assemblies[i].LoadBundle();
            yield return AddProgress(i, assemblies.Length, 0.3f);
        }
    }

    private static bool _isInitialized;
}
