// using System;
// using System.Collections.Generic;
// using System.Drawing;
// using System.Linq;
// using System.Reflection;
// using System.Runtime.InteropServices;
// using HarmonyLib;
// using Il2CppInterop.Common;
// using Il2CppInterop.HarmonySupport;
// using Il2CppInterop.Runtime;
// using Il2CppInterop.Runtime.Startup;
// using Mono.Cecil.Cil;
// using MonoMod.Cil;
// using MonoMod.Utils;
//
// namespace RedLoader.Utils.Hooking;
//
// public class HookDetourPatcher : Il2CppDetourMethodPatcher
// {
//     public static readonly Dictionary<long, HookPatchInstance> HarmonyPatchInstances = new();
//
//     private readonly HookPatchInstance _patchInstance;
//
//     public HookDetourPatcher(MethodBase original, HookPatchInstance patchInstance) : base(original)
//     {
//         _patchInstance = patchInstance;
//     }
//
//     public void Apply()
//     {
//         DetourTo(null);
//     }
//
//     public override MethodBase DetourTo(MethodBase _)
//     {
//         // // Unpatch an existing detour if it exists
//         if (nativeDetour != null)
//         {
//             // Point back to the original method before we unpatch
//             modifiedNativeMethodInfo.MethodPointer = originalNativeMethodInfo.MethodPointer;
//             nativeDetour.Dispose();
//         }
//
//         // Generate a new DMD of the modified unhollowed method, and apply harmony patches to it
//         var copiedDmd = CopyOriginal();
//
//         // Generate the MethodInfo instances
//         var managedHookedMethod = copiedDmd.Generate();
//         var unmanagedTrampolineMethod = GenerateNativeToManagedTrampoline(managedHookedMethod).Generate();
//
//         // Apply a detour from the unmanaged implementation to the patched harmony method
//         var unmanagedDelegateType = DelegateTypeFactory.instance.CreateDelegateType(unmanagedTrampolineMethod,
//             CallingConvention.Cdecl);
//
//         var unmanagedDelegate = unmanagedTrampolineMethod.CreateDelegate(unmanagedDelegateType);
//         DelegateCache.Add(unmanagedDelegate);
//
//         nativeDetour =
//             Il2CppInteropRuntime.Instance.DetourProvider.Create(originalNativeMethodInfo.MethodPointer, unmanagedDelegate);
//         nativeDetour.Apply();
//         modifiedNativeMethodInfo.MethodPointer = nativeDetour.OriginalTrampoline;
//
//         // TODO: Add an ILHook for the original unhollowed method to go directly to managedHookedMethod
//         // Right now it goes through three times as much interop conversion as it needs to, when being called from managed side
//         return managedHookedMethod;
//     }
//
//     public static void PostfixCaller(long ptr, object[] args)
//     {
//         var instance = HarmonyPatchInstances[ptr];
//         // for (var i = 0; i < args.Length; i++)
//         // {
//         //     var arg = args[i];
//         //     RLog.Msg(Color.CadetBlue, $"[{i}] PAR: {arg.ToString()}");
//         // }
//
//         foreach (var del in instance.PostfixDelegates)
//         {
//             //RLog.Msg(Color.CadetBlue, "Calling: " + instance.HookDelegates.First().Method.Name);
//             del.DynamicInvoke(args);
//         }
//     }
//
//     public static bool PrefixCaller(long ptr, object[] args)
//     {
//         var instance = HarmonyPatchInstances[ptr];
//
//         foreach (var del in instance.PrefixDelegates)
//         {
//             //RLog.Msg(Color.CadetBlue, "Calling: " + del.Method.Name);
//             var result = del.DynamicInvoke(args);
//             if (result is false)
//             {
//                 return false;
//             }
//         }
//         
//         return true;
//     }
//
//     public override DynamicMethodDefinition CopyOriginal()
//     {
//         var ptr = modifiedNativeMethodInfo.Pointer.ToInt64();
//         HarmonyPatchInstances[ptr] = _patchInstance;
//
//         var dmd = new DynamicMethodDefinition(Original);
//         dmd.Definition.Name = "UnhollowedWrapper_" + dmd.Definition.Name;
//         var cursor = new ILCursor(new ILContext(dmd.Definition));
//
//         CreateDelegateCall(dmd, cursor, ptr, typeof(HookDetourPatcher).GetMethod(nameof(PrefixCaller), BindingFlags.Static | BindingFlags.Public));
//         cursor.Emit(OpCodes.Brfalse, dmd.Definition.Body.Instructions.Last());
//
//         // Remove il2cpp_object_get_virtual_method
//         if (cursor.TryGotoNext(x => x.MatchLdarg(0),
//                 x => x.MatchCall(typeof(IL2CPP),
//                     nameof(IL2CPP.Il2CppObjectBaseToPtr)),
//                 x => x.MatchLdsfld(out _),
//                 x => x.MatchCall(typeof(IL2CPP),
//                     nameof(IL2CPP.il2cpp_object_get_virtual_method))))
//         {
//             cursor.RemoveRange(4);
//         }
//         else
//         {
//             cursor.Goto(0)
//                 .GotoNext(x =>
//                     x.MatchLdsfld(Il2CppInteropUtils
//                         .GetIl2CppMethodInfoPointerFieldForGeneratedMethod(Original)))
//                 .Remove();
//         }
//
//         // Replace original IL2CPPMethodInfo pointer with a modified one that points to the trampoline
//         cursor
//             .Emit(OpCodes.Ldc_I8, ptr)
//             .Emit(OpCodes.Conv_I);
//
//         CreateDelegateCall(dmd, cursor, ptr, typeof(HookDetourPatcher).GetMethod(nameof(PostfixCaller), BindingFlags.Static | BindingFlags.Public));
//
//         //RLog.Msg(Color.Orange, $"IL: {String.Join("\n", dmd.Definition.Body.Instructions.Select(x => x.ToString()))}");
//
//         return dmd;
//     }
//
//     private static void CreateDelegateCall(DynamicMethodDefinition dmd, ILCursor cursor, long ptr, MethodBase method)
//     {
//         var pars = dmd.Definition.Parameters;
//
//         // load pointer parameter
//         cursor.Emit(OpCodes.Ldc_I8, ptr)
//             .Emit(OpCodes.Ldc_I4, pars.Count);
//         
//         // create array of objects
//         cursor.Emit(OpCodes.Newarr, typeof(object));
//
//         // load original method parameters into array
//         for (var i = 0; i < pars.Count; i++)
//         {
//             //RLog.Msg(Color.Orange, $"[{i}] Loading in parameter {pars[i].Name}");
//
//             cursor.Emit(OpCodes.Dup)
//                 .Emit(OpCodes.Ldc_I4, i)
//                 .Emit(OpCodes.Ldarg, pars[i]);
//
//             if (pars[i].ParameterType.IsValueType)
//             {
//                 cursor.Emit(OpCodes.Box, pars[i].ParameterType);
//             }
//
//             cursor.Emit(OpCodes.Stelem_Ref);
//         }
//
//         // Call method
//         cursor.Emit(OpCodes.Call, method);
//     }
// }