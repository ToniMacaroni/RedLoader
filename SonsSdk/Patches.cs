using System.Reflection;
using HarmonyLib;
using Sons.Cutscenes;

namespace SonsSdk;

public static class Patches
{
    public static HarmonyLib.Harmony Harmony { get; private set; }
    
    internal static void InitPatches()
    {
        if (_isInitialized)
        {
            return;
        }

        Harmony = new HarmonyLib.Harmony("SonsSdk");
        
        InitPostPatch<CutsceneManager>(nameof(CutsceneManager.TriggerOpeningCutsceneInternal), nameof(PatchTriggerOpeningCutscene));

        _isInitialized = true;
    }

    // Patch opening cutscene (to trigger OnGameStart)
    private static void PatchTriggerOpeningCutscene()
    {
        SdkEvents.OnGameStart.Invoke();
    }

    internal static void InitPostPatch<T>(string methodName, string overrideMethodName)
    {
        Harmony.Patch(
            typeof(T).GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public), 
            null, 
            new HarmonyMethod(typeof(Patches).GetMethod(overrideMethodName, BindingFlags.Static | BindingFlags.NonPublic)));
    }
    
    internal static void InitPrePatch<T>(string methodName, string overrideMethodName)
    {
        Harmony.Patch(
            typeof(T).GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public),
            new HarmonyMethod(typeof(Patches).GetMethod(overrideMethodName, BindingFlags.Static | BindingFlags.NonPublic)));
    }
    
    private static bool _isInitialized;
}