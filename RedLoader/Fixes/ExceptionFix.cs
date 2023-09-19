using System;
using System.Diagnostics;
using HarmonyLib;
using RedLoader.Assertions;

namespace RedLoader.Fixes;

internal class ExceptionFix
{
    public static void Install()
    {
        var stackTracePatch = AccessTools.Method(typeof(ExceptionFix), nameof(StackTracePatch)).ToNewHarmonyMethod();
        var exceptionPatch = AccessTools.Method(typeof(ExceptionFix), nameof(ExceptionPatch)).ToNewHarmonyMethod();

        var stackTraceMethod = AccessTools.PropertyGetter(typeof(Exception), nameof(Exception.StackTrace));
        var exceptionMethod = AccessTools.Method(typeof(Exception), nameof(Exception.ToString));
        
        RAssert.IsNotNull(stackTracePatch);
        RAssert.IsNotNull(exceptionPatch);
        
        RAssert.IsNotNull(stackTraceMethod);
        RAssert.IsNotNull(exceptionMethod);

        try
        {
            Core.HarmonyInstance.Patch(stackTraceMethod, stackTracePatch);
            Core.HarmonyInstance.Patch(exceptionMethod, exceptionPatch);
        }
        catch (Exception e)
        {
            RLog.Error($"{nameof(ExceptionFix)} Exception: {e}");
        }
    }
    
    private static bool StackTracePatch(Exception __instance, ref string __result)
    {
        try
        {
            var exStackTrace = new EnhancedStackTrace(__instance);
            __result = exStackTrace.ToString();
            return false;
        }
        catch (Exception)
        {
            return true;
        }
    }
    
    private static bool ExceptionPatch(Exception __instance, ref string __result)
    {
        __result = __instance.ToStringDemystified();
        return false;
    }
}