using System;
using Il2CppInterop.Runtime.Injection;

namespace RedLoader.Utils;

public class InjectorEx
{
    public static void Inject<T>(params Type[] interfaces) where T : class
    {
        ClassInjector.RegisterTypeInIl2Cpp<T>(new()
        {
            LogSuccess = true,
            Interfaces = interfaces
        });
    }
}