using System.Collections;
using MelonLoader;
using MelonLoader.Utils;
using Sons.Input;
using Sons.Loading;
using TheForest.Utils;
using UnityEngine.SceneManagement;

namespace SonsSdk;

internal class MainInitializer
{
    internal static void InitTitleScene()
    {
        // if (MelonLaunchOptions.SonsSdk.LoadIntoMain)
        // {
        //     SdkEvents.OnSdkInitialized.Invoke();
        //     return;
        // }
        
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        
        DelayedTitleLoad().RunCoro();
    }
    
    private static IEnumerator DelayedTitleLoad()
    {
        SceneManager.LoadScene(SonsSceneManager.OptionsMenuSceneName, LoadSceneMode.Additive);
        yield return null;
        SUI.SUI.InitPrefabs();
        SceneManager.UnloadScene(SonsSceneManager.OptionsMenuSceneName);
        
        SdkEvents.OnSdkInitialized.Invoke();
    }
    
    private static bool _isInitialized;
}