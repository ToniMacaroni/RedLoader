using HarmonyLib;
using UnityEngine;
using Color = System.Drawing.Color;

namespace RedLoader;

public static class UnityPatches
{
    public static void CreateAndApply()
    {
        HarmonyLib.Harmony.CreateAndPatchAll(typeof(UnityPatches));
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Debug), nameof(Debug.Log), typeof(UnityEngine.Object))]
    private static void LogPatch(Object message) => RLog.MsgDirect(Color.DarkGray, $"[Unity] {message.ToString()}");
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Debug), nameof(Debug.LogWarning), typeof(UnityEngine.Object))]
    private static void LogWarningPatch(Object message) => RLog.MsgDirect(Color.Yellow, $"[Unity] {message.ToString()}");
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Debug), nameof(Debug.LogError), typeof(UnityEngine.Object))]
    private static void LogErrorPatch(Object message) => RLog.MsgDirect(Color.IndianRed, $"[Unity] {message.ToString()}");
}
