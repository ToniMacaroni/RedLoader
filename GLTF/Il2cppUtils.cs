using System.Reflection;
using System.Runtime.InteropServices;
using Il2CppInterop.Common;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Unity.Collections;

namespace GLTF;

public static class Il2cppUtils
{
    private static IntPtr _bufferField;
    
    private static IntPtr GetBufferField()
    {
        if (_bufferField == IntPtr.Zero)
        {
            _bufferField = (IntPtr)Il2CppInteropUtils.GetIl2CppFieldInfoPointerFieldForGeneratedFieldAccessor(typeof(NativeArray<byte>)
                .GetProperty("m_Buffer", BindingFlags.Public | BindingFlags.Instance)!.GetMethod).GetValue(new NativeArray<byte>())!;
        }

        return _bufferField;
    }

    public static unsafe void* GetUnsafePtrAlt<T>(this NativeArray<T> nativeArray) where T : new()
    {
        return *(void**)(IL2CPP.Il2CppObjectBaseToPtrNotNull(nativeArray) + (int)IL2CPP.il2cpp_field_get_offset(GetBufferField()));
    }
    
    public static unsafe NativeArray<byte> NewNativeArray(byte[] data, Allocator allocator)
    {
        var nativeArray = new NativeArray<byte>(data.Length, allocator);
        var ptr = GetUnsafePtrAlt(nativeArray);
        Marshal.Copy(data, 0, (IntPtr)ptr, data.Length);
        return nativeArray;
    }
    
    public static unsafe NativeArray<T> NewNativeArray<T>(T[] data, Allocator allocator) where T : new()
    {
        var nativeArray = new NativeArray<T>(data.Length, allocator);
        var ptr = (IntPtr)GetUnsafePtrAlt(nativeArray);
        var sz = Marshal.SizeOf<T>();
        
        for (var i = 0; i < data.Length; i++)
        {
            Marshal.StructureToPtr(data[i], ptr, false);
            ptr += sz;
        }
        
        return nativeArray;
    }

    public static unsafe T* GetRaw<T>(this Il2CppStructArray<T> array) where T : unmanaged
    {
        return (T*)IntPtr.Add(array.Pointer, 4 * IntPtr.Size);
    }
}