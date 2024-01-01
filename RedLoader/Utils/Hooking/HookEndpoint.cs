using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using MonoMod.RuntimeDetour;

namespace RedLoader.Utils;

internal sealed class HookEndpoint
{
    internal HookEndpoint(MethodBase method, HarmonyLib.Harmony harmonyInstance)
    {
        Method = method;
        _harmonyInstance = harmonyInstance;
    }

    public void _Remove(Delegate hookDelegate)
    {
        if (hookDelegate == null)
        {
            return;
        }

        // Note: A hook delegate can be applied multiple times.
        // The following code removes the last hook of that delegate type.
        if (!HookMap.TryGetValue(hookDelegate, out Stack<HarmonyDelegatePatch> hooks))
        {
            return;
        }

        var hook = hooks.Pop();
        hook.Dispose();

        if (hooks.Count == 0)
        {
            HookMap.Remove(hookDelegate);
        }

        HookList.Remove(hook);
    }

    public void Add(Delegate hookDelegate, bool prefix)
    {
        _Add(_NewHook, hookDelegate, prefix);
    }

    public void Remove(Delegate hookDelegate)
    {
        _Remove(hookDelegate);
    }

    private static HarmonyDelegatePatch _NewHook(HarmonyLib.Harmony harmonyInstance, MethodBase from, Delegate to, bool prefix)
    {
        return new HarmonyDelegatePatch(harmonyInstance, from, to, prefix);
    }

    private void _Add<TDelegate>(Func<HarmonyLib.Harmony, MethodBase, TDelegate, bool, HarmonyDelegatePatch> gen, TDelegate hookDelegate, bool prefix) where TDelegate : Delegate
    {
        if (hookDelegate == null)
        {
            return;
        }

        if (!HookMap.TryGetValue(hookDelegate, out Stack<HarmonyDelegatePatch> hooks))
        {
            HookMap[hookDelegate] = hooks = new Stack<HarmonyDelegatePatch>();
        }

        var hook = gen(_harmonyInstance, Method, hookDelegate, prefix);
        hooks.Push(hook);
        HookList.Add(hook);
    }

    private readonly Dictionary<Delegate, Stack<HarmonyDelegatePatch>> HookMap = new();
    private readonly List<HarmonyDelegatePatch> HookList = new();

    internal readonly MethodBase Method;
    private readonly HarmonyLib.Harmony _harmonyInstance;
}

public class HarmonyDelegatePatch : IDisposable
{
    private readonly HarmonyLib.Harmony _harmonyInstance;
    private MethodBase _originalMethod;
    private Delegate _toDelegate;

    public HarmonyDelegatePatch(HarmonyLib.Harmony harmonyInstance, MethodBase originalMethod, Delegate toDelegate, bool prefix)
    {
        _harmonyInstance = harmonyInstance;
        _originalMethod = originalMethod;
        _toDelegate = toDelegate;

        RLog.Msg(Color.Magenta, "--> Hooking {0} with {1}", originalMethod, toDelegate);
        _harmonyInstance.Patch(originalMethod, prefix ? new HarmonyLib.HarmonyMethod(toDelegate.Method) : null, prefix ? null : new HarmonyLib.HarmonyMethod(toDelegate.Method));
    }

    public void Dispose()
    {
        RLog.Msg(Color.Magenta, "--> Unhooking {0} with {1}", _originalMethod, _toDelegate);
        _harmonyInstance.Unpatch(_originalMethod, _toDelegate.Method);
    }
}