#pragma warning disable CS0649 //Field is never assigned (native struct)
namespace NativeHost
{
    unsafe struct HostExports
    {
        internal delegate* unmanaged<void**, void*, void> HookAttach;
        internal delegate* unmanaged<void**, void*, void> HookDetach;
    }
}
