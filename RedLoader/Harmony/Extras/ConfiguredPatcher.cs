using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Harmony;

public class ConfiguredPatcher
{
    public enum EMethodType
    {
        PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static,
        PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance,
        PublicStatic = BindingFlags.Public | BindingFlags.Static,
        PublicInstance = BindingFlags.Public | BindingFlags.Instance,
    }
}

public class ConfiguredPatcher<T> : ConfiguredPatcher
{
    private const BindingFlags PRIVATE_STATIC = BindingFlags.NonPublic | BindingFlags.Static;

    private readonly HarmonyLib.Harmony _harmony;
    
    private readonly Dictionary<string, HarmonyLib.HarmonyMethod> _harmonyMethods = new Dictionary<string, HarmonyLib.HarmonyMethod>();

    public ConfiguredPatcher(HarmonyLib.Harmony harmony)
    {
        _harmony = harmony;
    }

    public void Prefix<T2>(string sourceMethodName, string targetMethodName, bool shouldPatch = true, params Type[] parameters)
    {
        Prefix(typeof(T2), sourceMethodName, targetMethodName, shouldPatch, parameters);
    }
    
    public void Prefix(Type type, string sourceMethodName, string targetMethodName, bool shouldPatch = true, params Type[] parameters)
    {
        if(!shouldPatch)
            return;
        
        var (sourceMethod, harmonyMethod) = GetMethods(type, sourceMethodName, targetMethodName, parameters);
        _harmony.Patch(sourceMethod, harmonyMethod);
    }

    public void Postfix<T2>(string sourceMethodName, string targetMethodName, bool shouldPatch = true, params Type[] parameters)
    {
        Postfix(typeof(T2), sourceMethodName, targetMethodName, shouldPatch, parameters);
    }
    
    public void Postfix(Type type, string sourceMethodName, string targetMethodName, bool shouldPatch = true, params Type[] parameters)
    {
        if(!shouldPatch)
            return;
        
        var (sourceMethod, harmonyMethod) = GetMethods(type, sourceMethodName, targetMethodName, parameters);
        _harmony.Patch(sourceMethod, null, harmonyMethod);
    }

    public (MethodBase sourceMethod, HarmonyMethod targetMethod) GetMethods(Type type, string sourceMethodName, string targetMethodName, params Type[] parameters)
    {
        var harmonyMethod = GetTargetMethod(targetMethodName);
        MethodInfo sourceMethod;
        if(parameters.Length == 0)
            sourceMethod = HarmonyLib.AccessTools.Method(type, sourceMethodName);
        else
            sourceMethod = HarmonyLib.AccessTools.Method(type, sourceMethodName, parameters);
        if (sourceMethod == null)
            throw new MissingMethodException(type.FullName, sourceMethodName);
        
        return (sourceMethod, harmonyMethod);
    }
    
    public void UnpatchAll()
    {
        _harmony.UnpatchSelf();
    }

    private HarmonyLib.HarmonyMethod GetTargetMethod(string methodName)
    {
        if (_harmonyMethods.TryGetValue(methodName, out var harmonyMethod))
            return harmonyMethod;
        
        var method = typeof(T).GetMethod(methodName, PRIVATE_STATIC);
        if (method == null)
            throw new MissingMethodException(typeof(T).FullName, methodName);
        
        harmonyMethod = new HarmonyLib.HarmonyMethod(method);
        _harmonyMethods.Add(methodName, harmonyMethod);
        return harmonyMethod;
    }
}