using System.Reflection;
using System.Reflection.Emit;
using Object = UnityEngine.Object;

namespace SonsSdk.Attributes;

internal static class DynamicInitializerStore
{
    private static readonly Dictionary<Type, Func<IntPtr, Object>> Initializers = new();

    public static Func<IntPtr, Object> GetInitializer(Type t)
    {
        if (Initializers.TryGetValue(t, out var func))
        {
            return func;
        }

        func = Create(t);
        Initializers.Add(t, func);
        return func;
    }

    private static Func<IntPtr, Object> Create(Type type)
    {
        var dynamicMethod = new DynamicMethod("Initializer<" + type.AssemblyQualifiedName + ">", type, new[] { typeof(IntPtr) });
        dynamicMethod.DefineParameter(0, ParameterAttributes.None, "pointer");

        var ilGenerator = dynamicMethod.GetILGenerator();
        var constructor1 = type.GetConstructor(new[] { typeof(IntPtr) });

        if (constructor1 != null)
        {
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Newobj, constructor1);
        }

        ilGenerator.Emit(OpCodes.Ret);
        return dynamicMethod.CreateDelegate<Func<IntPtr, Object>>();
    }
}