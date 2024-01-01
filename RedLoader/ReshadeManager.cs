using System.Runtime.InteropServices;

namespace RedLoader;

internal class ReshadeManager
{
    [DllImport("ReShade64.dll")]
    public static extern bool LoadUnity();
}