using System;
using System.ComponentModel;

namespace RedLoader.Preloader.RuntimeFixes;

public static class HarmonyBackendFix
{
    private static readonly MonoModBackend ConfigHarmonyBackend = MonoModBackend.auto;
    
    public static void Initialize()
    {
        switch (ConfigHarmonyBackend)
        {
            case MonoModBackend.auto:
                break;
            case MonoModBackend.dynamicmethod:
            case MonoModBackend.methodbuilder:
            case MonoModBackend.cecil:
                Environment.SetEnvironmentVariable("MONOMOD_DMD_TYPE", ConfigHarmonyBackend.ToString());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(ConfigHarmonyBackend), ConfigHarmonyBackend,
                                                      "Unknown backend");
        }
    }

    private enum MonoModBackend
    {
        // Enum names are important!
        [Description("Auto")]
        auto = 0,

        [Description("DynamicMethod")]
        dynamicmethod,

        [Description("MethodBuilder")]
        methodbuilder,

        [Description("Cecil")]
        cecil
    }
}
