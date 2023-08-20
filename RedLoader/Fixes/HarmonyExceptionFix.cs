using System;
using HarmonyLib;

namespace RedLoader.Fixes;

internal static class HarmonyExceptionFix
{
    internal static void Install()
    {
        var patchMethod = AccessTools.Method(typeof(HarmonyExceptionFix), "PatchMethod").ToNewHarmonyMethod();

        try
        {
            Core.HarmonyInstance.Patch(AccessTools.Method("Il2CppInterop.HarmonySupport.Il2CppDetourMethodPatcher:ReportException"), patchMethod);
        }
        catch (Exception ex) { RLog.Warning($"{nameof(HarmonyExceptionFix)} Exception: {ex}"); }
    }
    
    private static bool PatchMethod(Exception __0)
    {
        RLog.Error(__0);
        return false;
    }
}