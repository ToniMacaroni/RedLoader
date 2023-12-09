using System;
using System.Runtime.InteropServices;
using HarmonyLib;
using Il2CppInterop.Common;

namespace RedLoader.Utils;

public class Reflow
{
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize,
        Protection flNewProtect, out Protection lpflOldProtect);

    public enum Protection {
        PAGE_NOACCESS = 0x01,
        PAGE_READONLY = 0x02,
        PAGE_READWRITE = 0x04,
        PAGE_WRITECOPY = 0x08,
        PAGE_EXECUTE = 0x10,
        PAGE_EXECUTE_READ = 0x20,
        PAGE_EXECUTE_READWRITE = 0x40,
        PAGE_EXECUTE_WRITECOPY = 0x80,
        PAGE_GUARD = 0x100,
        PAGE_NOCACHE = 0x200,
        PAGE_WRITECOMBINE = 0x400
    }
    
    public static void WriteMemory(nint address, byte[] data, bool changeProtection = true)
    {
        var size = data.Length;
        var old = Protection.PAGE_EXECUTE_READ;

        if(changeProtection)
            VirtualProtect(address, (uint)size, Protection.PAGE_EXECUTE_READWRITE, out old);
        
        Marshal.Copy(data, 0, address, size);

        if(changeProtection)
            VirtualProtect(address, (uint)size, old, out _);
    }

    public static void NopMemory(nint address, int length, bool changeProtection = true)
    {
        var patch = new byte[length];
        Array.Fill(patch, (byte)0x90, 0, length);

        Protection old = Protection.PAGE_EXECUTE_READ;

        if(changeProtection)
            VirtualProtect(address, (uint)length, Protection.PAGE_EXECUTE_READWRITE, out old);
        
        Marshal.Copy(patch, 0, address, length);

        if(changeProtection)
            VirtualProtect(address, (uint)length, old, out _);
    }

    public static unsafe nint GetInMethod<T>(string methodName, char[] pattern, char[] mask, long sigOffset = 0, int blockSize = 500)
    {
        var methodPtr = GetMethodEntry<T>(methodName);
        return FindSignatureInBlock(methodPtr, blockSize, pattern, mask, sigOffset);
    }

    public static unsafe nint GetMethodEntry<T>(string methodName)
    {
        var methodBase = AccessTools.Method(typeof(T), methodName);
        if (methodBase == null)
        {
            throw new Exception($"Failed to find method: {methodName}");
        }
        
        return *(nint*)(nint)Il2CppInteropUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(methodBase).GetValue(null)!;
    }
    
    public static unsafe nint GetMethodEntry<T>(string methodName, params Type[] parameters)
    {
        var methodBase = AccessTools.Method(typeof(T), methodName, parameters);
        if (methodBase == null)
        {
            throw new Exception($"Failed to find method: {methodName}");
        }
        
        return *(nint*)(nint)Il2CppInteropUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(methodBase).GetValue(null)!;
    }

    public static unsafe nint FindSignatureInBlock(nint block, long blockSize, char[] pattern, char[] mask,
        long sigOffset = 0)
    {
        for (long address = 0; address < blockSize; address++)
        {
            var found = true;
            for (uint offset = 0; offset < mask.Length; offset++)
            {
                if (*(byte*)(address + block + offset) != (byte)pattern[offset] && mask[offset] != '?')
                {
                    found = false;
                    break;
                }
            }

            if (found)
                return (nint)(address + block + sigOffset);
        }

        return 0;
    }
}