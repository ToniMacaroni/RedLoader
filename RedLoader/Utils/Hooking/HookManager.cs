// using System;
// using System.Collections.Generic;
// using System.Reflection;
// using HarmonyLib.Public.Patching;
//
// namespace RedLoader.Utils;
//
// public static class HookManager
// {
//     internal static Dictionary<MethodBase, HookPatchInstance> HarmonyPatchInstances = new();
//     
//     private static HarmonyLib.Harmony _harmonyInstance;
//
//     public static void AddPrefix(MethodBase method, Delegate hookDelegate)
//     {
//         Add(method, hookDelegate, true);
//     }
//     
//     public static void AddPostfix(MethodBase method, Delegate hookDelegate)
//     {
//         Add(method, hookDelegate, false);
//     }
//
//     public static void Add(MethodBase method, Delegate hookDelegate, bool prefix)
//     {
//         if(!HarmonyPatchInstances.TryGetValue(method, out var instance))
//         {
//             var harmony = GetOrCreateHarmony();
//             instance = new HookPatchInstance(method);
//             HarmonyPatchInstances[method] = instance;
//             //harmony.Patch(method, prefix ? new HarmonyLib.HarmonyMethod(generatedDelegate) : null, prefix ? null : new HarmonyLib.HarmonyMethod(generatedDelegate));
//             instance.Add(hookDelegate, prefix);
//         }
//         else
//         {
//             instance.Add(hookDelegate, prefix);
//         }
//     }
//
//     public static void Remove(MethodBase method, Delegate hookDelegate)
//     {
//         if(!HarmonyPatchInstances.TryGetValue(method, out var instance))
//             return;
//         
//         instance.Remove(hookDelegate);
//     }
//
//     private static HarmonyLib.Harmony GetOrCreateHarmony()
//     {
//         return _harmonyInstance ??= new HarmonyLib.Harmony("RedLoader.Hooks");
//     }
// }