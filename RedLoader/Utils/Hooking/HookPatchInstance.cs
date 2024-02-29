using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RedLoader.Utils.Hooking;

namespace RedLoader.Utils;

public class HookPatchInstance
{
    internal static readonly Dictionary<MethodBase, HookPatchInstance> HarmonyPatchInstances = new();
    
    public readonly HashSet<Delegate> PostfixDelegates = new();
    public readonly HashSet<Delegate> PrefixDelegates = new();

    public HookPatchInstance(MethodBase originalMethod)
    {
        HarmonyPatchInstances[originalMethod] = this;

        var patcher = new HookDetourPatcher(originalMethod, this);
        patcher.Apply();
    }

    public void Add(Delegate hookDelegate, bool prefix)
    {
        if (hookDelegate == null)
        {
            return;
        }
        
        if (prefix)
        {
            if (PrefixDelegates.Contains(hookDelegate))
            {
                return;
            }

            PrefixDelegates.Add(hookDelegate);
            return;
        }
        
        if (PostfixDelegates.Contains(hookDelegate))
        {
            return;
        }

        PostfixDelegates.Add(hookDelegate);
    }
    
    public void Remove(Delegate hookDelegate)
    {
        if (hookDelegate == null)
        {
            return;
        }

        PostfixDelegates.Remove(hookDelegate);
    }
}