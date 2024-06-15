using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using UnityEngine;

namespace RedLoader.Utils;

public static class CommonExtensions
{
    public static bool IsVectorNearZero(this Vector3 vec)
    {
        return Mathf.Abs(vec.x) < 0.01f && Mathf.Abs(vec.y) < 0.01f && Mathf.Abs(vec.z) < 0.01f;
    }
    
    public static bool IsPositionNearZero(this Transform tr)
    {
        return IsVectorNearZero(tr.position);
    }
    
    public static bool IsAssignableFrom<T>(this Il2CppObjectBase obj)
    {
        var classPtr = Il2CppClassPointerStore<T>.NativeClassPtr;
        return IL2CPP.il2cpp_class_is_assignable_from(classPtr, obj.ObjectClass);
    }
}