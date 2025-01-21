using Harmony;
using Sons.Multiplayer.Dedicated;

namespace SonsSdk;

internal class MiscPatches
{
    private static ConfiguredPatcher<MiscPatches> _patcher;
    
    internal static void Init()
    {
        _patcher = new ConfiguredPatcher<MiscPatches>(SdkEntryPoint.Harmony);
        _patcher.Prefix<SonsLaunch>(nameof(SonsLaunch.Start), nameof(SonsLaunchStart));
    }

    private static void SonsLaunchStart(SonsLaunch __instance)
    {
        __instance._titleSceneLoader._delay = 0f;
    }
}
