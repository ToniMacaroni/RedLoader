﻿using System.Runtime.InteropServices;

namespace RedLoader.NativeUtils.PEParser
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ImageThunkData64
    {
        [FieldOffset(0)]
        public ulong forwarderString;
        [FieldOffset(0)]
        public ulong function;
        [FieldOffset(0)]
        public ulong ordinal;
        [FieldOffset(0)]
        public ulong addressOfData;
    }
}
