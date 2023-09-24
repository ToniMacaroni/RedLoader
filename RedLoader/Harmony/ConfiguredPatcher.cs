using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RedLoader;

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
    
    public void Patch(string methodName, bool shouldPatch = true)
    {
        if (!shouldPatch)
            return;

        var targetMethod = AccessTools.Method(typeof(T), methodName);
        if (targetMethod == null)
            throw new Exception($"Could not find method {methodName} in type {typeof(T).FullName}");

        var isPrefix = targetMethod.Name.EndsWith("Prefix");
        var sourceMethod = GetMethodFromAttribute(targetMethod);
        _harmony.Patch(sourceMethod, isPrefix ? targetMethod.ToNewHarmonyMethod() : null, isPrefix ? null : targetMethod.ToNewHarmonyMethod());
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

    public void Patch(Type type, bool shouldPatch = true)
    {
        if(!shouldPatch)
            return;
        
        _harmony.PatchAll(type);
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
    
    private MethodBase GetMethodFromAttribute(MethodBase method)
    {
        var attr = method.GetCustomAttribute<HarmonyPatch>();
        if (attr == null)
            throw new Exception($"Method {method.Name} in type {typeof(T).FullName} is not a Harmony patch");
        var info = attr.info;

        MethodBase sourceMethod = null;

        if (!info.methodType.HasValue)
            sourceMethod = AccessTools.Method(info.declaringType, info.methodName, info.argumentTypes);
        else if(info.methodType.Value == MethodType.Setter)
            sourceMethod = AccessTools.PropertySetter(info.declaringType, info.methodName);
        else if(info.methodType.Value == MethodType.Getter)
            sourceMethod = AccessTools.PropertyGetter(info.declaringType, info.methodName);
        else if(info.methodType.Value == MethodType.Constructor)
            sourceMethod = AccessTools.Constructor(info.declaringType, info.argumentTypes);
        else if(info.methodType.Value == MethodType.StaticConstructor)
            sourceMethod = AccessTools.Constructor(info.declaringType, info.argumentTypes);
        
        if (sourceMethod == null)
            throw new MissingMethodException(info.declaringType.FullName, info.methodName);
        
        return sourceMethod;
    }
}