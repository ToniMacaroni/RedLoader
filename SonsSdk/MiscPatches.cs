using System.Drawing;
using Harmony;
using RedLoader;
using RedLoader.Utils;
using Sons.Multiplayer;
using Sons.Multiplayer.Dedicated;
using Steamworks;

namespace SonsSdk;

internal class MiscPatches
{
    private static ConfiguredPatcher<MiscPatches> _patcher;
    
    internal static void Init()
    {
        _patcher = new ConfiguredPatcher<MiscPatches>(SdkEntryPoint.Harmony);
        _patcher.Prefix<SonsLaunch>(nameof(SonsLaunch.Start), nameof(SonsLaunchStart));

        if (LoaderEnvironment.IsDedicatedServer)
        {
            DoServerPatches();
        }
        else
        {
            DoGamePatches();
        }
    }

    private static void DoGamePatches()
    {
        
    }

    private static void DoServerPatches()
    {
        PatchMaxPlayerLimit();
        _patcher.Prefix<DedicatedServerBoostrap>(nameof(DedicatedServerBoostrap.CreateDedicated), nameof(BeforeServerCreation));
        _patcher.Prefix(typeof(SteamGameServer), nameof(SteamGameServer.SetServerName), nameof(AlterServerName));
    }

    private static void SonsLaunchStart(SonsLaunch __instance)
    {
        __instance._titleSceneLoader._delay = 0f;
    }
    
    private static void BeforeServerCreation(DedicatedServerBoostrap __instance)
    { }
    
    private static void AlterServerName(ref string pszServerName)
    {
        pszServerName = $"<color=red>[MODDED]</color> {pszServerName}";
    }
    
    public static unsafe void PatchMaxPlayerLimit()
    {
        var address = Reflow.GetInMethod<ServerDedicatedConfig>(
                                                                nameof(ServerDedicatedConfig.Validate), 
                                                                "\x8B\x40\x28\x83\xF8\x08".ToCharArray(),
                                                                "?x?xxx".ToCharArray(), 3);

        if(address == 0)
            throw new Exception("Failed to find patch address");
        
        RLog.Msg(Color.Pink, $"Player Limit Address: {address.ToString("X")}");

        Reflow.NopMemory(address, 9);
    }
}
