using System;
using System.Collections.Generic;
using System.Reflection;

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

    public void Prefix<T2>(string sourceMethodName, string targetMethodName, params Type[] parameters)
    {
        var harmonyMethod = GetTargetMethod(targetMethodName);
        // var sourceMethod = typeof(T2).GetMethod(sourceMethodName, BindingFlags.NonPublic | BindingFlags.Instance);
        MethodInfo sourceMethod;
        if(parameters.Length == 0)
            sourceMethod = HarmonyLib.AccessTools.Method(typeof(T2), sourceMethodName);
        else
            sourceMethod = HarmonyLib.AccessTools.Method(typeof(T2), sourceMethodName, parameters);
        if (sourceMethod == null)
            throw new MissingMethodException(typeof(T2).FullName, sourceMethodName);
        
        _harmony.Patch(sourceMethod, harmonyMethod);
    }
    
    public void Postfix<T2>(string sourceMethodName, string targetMethodName)
    {
        var harmonyMethod = GetTargetMethod(targetMethodName);
        var sourceMethod = HarmonyLib.AccessTools.Method(typeof(T2), sourceMethodName);
        if (sourceMethod == null)
            throw new MissingMethodException(typeof(T2).FullName, sourceMethodName);
        
        _harmony.Patch(sourceMethod, null, harmonyMethod);
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