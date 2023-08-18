using System.Runtime.InteropServices;

namespace RedLoader.NativeUtils.PEParser
{
    [StructLayout(LayoutKind.Explicit)]
    public struct OptionalFileHeader64
    {
        [FieldOffset(112)]
        public ImageDataDirectory exportTable;
        [FieldOffset(128)]
        public ImageDataDirectory resourceTable;
    }
}