using System;
using HarmonyLib;

namespace SFLoader.Fixes;

internal static class HarmonyExceptionFix
{
    internal static void Install()
    {
        var patchMethod = AccessTools.Method(typeof(HarmonyExceptionFix), "PatchMethod").ToNewHarmonyMethod();

        try
        {
            Core.HarmonyInstance.Patch(AccessTools.Method("Il2CppInterop.HarmonySupport.Il2CppDetourMethodPatcher:ReportException"), patchMethod);
        }
        catch (Exception ex) { MelonLogger.Warning($"{nameof(HarmonyExceptionFix)} Exception: {ex}"); }
    }
    
    private static bool PatchMethod(Exception __0)
    {
        MelonLogger.Error(__0);
        return false;
    }
}