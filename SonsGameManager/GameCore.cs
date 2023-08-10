using Harmony;
using MelonLoader;
using SonsSdk;
using TheForest;
using UnityEngine;
using Object = System.Object;

namespace SonsGameManager;

public static class GameCore
{
    internal static bool ShouldLoadIntoMain;
    internal static bool SkipIntro = true;
    internal static bool RedirectDebugLogs = true;
    
    public static void Entry()
    {
        ShouldLoadIntoMain = MelonLaunchOptions.SonsSdk.LoadIntoMain;
        GamePatches.Init();
    }
    
    public static void EnableDebugConsole()
    {
        var debugConsole = UnityEngine.Object.FindObjectOfType<DebugConsole>();
        if (!debugConsole)
        {
            GameObject obj = new GameObject("DebugConsole");
            obj.AddComponent<DebugConsole>();
            obj.DontDestroyOnLoad();
            Debug.Log("Enabling debug console");
        }
    }
}