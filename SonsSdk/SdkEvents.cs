using System.Drawing;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using Sons.Cutscenes;

namespace SonsSdk;

internal static class SdkEvents
{
    public static readonly MelonEvent OnGameStart = new();
    
    private static HarmonyLib.Harmony _harmony;
    private static bool _isInitialized;
    
    internal static void Init()
    {
        if (_isInitialized)
        {
            return;
        }
        
        _harmony = new HarmonyLib.Harmony("SonsSdk");
        InitPatches();
        
        _isInitialized = true;
    }

    internal static void InitPatches()
    {
        InitPostPatch<CutsceneManager>(nameof(CutsceneManager.TriggerOpeningCutsceneInternal), nameof(PatchTriggerOpeningCutscene));
    }

    internal static void InitPostPatch<T>(string methodName, string overrideMethodName)
    {
        _harmony.Patch(
            typeof(T).GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public), 
            null, 
            new HarmonyMethod(typeof(SdkEvents).GetMethod(overrideMethodName, BindingFlags.Static | BindingFlags.NonPublic)));
    }

    private static void PatchTriggerOpeningCutscene()
    {
        OnGameStart.Invoke();
    }
}