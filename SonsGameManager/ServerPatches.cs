using System.Reflection;
using System.Runtime.InteropServices;
using Bolt;
using Endnight.Editor;
using Endnight.Utilities.Configurations;
using Endnight.Utilities.Dedicated;
using Harmony;
using HarmonyLib;
using Il2CppInterop.Common;
using Il2CppInterop.Runtime.Injection;
using RedLoader;
using RedLoader.Utils;
using Sons.Gui;
using Sons.Multiplayer;
using Sons.Multiplayer.Dedicated;
using Sons.Music;
using Sons.Save;
using Sons.TerrainDetail;
using SonsSdk;
using Steamworks;
using UdpKit;
using UnityEngine;
using Color = System.Drawing.Color;
using Object = Il2CppSystem.Object;

// ReSharper disable InconsistentNaming

namespace SonsGameManager;

public class ServerPatches
{
    private static ConfiguredPatcher<ServerPatches> _patcher;

    public static void Init()
    {
        _patcher = new(Core.HarmonyInst);
        
        _patcher.Prefix<SonsLaunch>("Start", nameof(LaunchStartPatch));
        
        // ClassInjector.RegisterTypeInIl2Cpp<RedToken>(new()
        // {
        //     Interfaces = new Type[]{typeof(IProtocolToken)}
        // });
        
        //BoltNetwork.RegisterTokenClass<RedToken>();
        
        if (Config.RedirectDebugLogs.Value)
        {
            _patcher.Prefix<Debug>(nameof(Debug.Log), nameof(LogPatch), true, typeof(Object));
            _patcher.Prefix<Debug>(nameof(Debug.LogWarning), nameof(LogWarningPatch), true, typeof(Object));
            _patcher.Prefix<Debug>(nameof(Debug.LogError), nameof(LogErrorPatch), true, typeof(Object));
        }

        PatchMaxPlayerLimit();

        _patcher.Prefix<DedicatedServerBoostrap>(nameof(DedicatedServerBoostrap.CreateDedicated), nameof(BeforeServerCreation));
        _patcher.Prefix(typeof(SteamGameServer), nameof(SteamGameServer.SetServerName), nameof(AlterServerName));

        RLog.Msg(SysColor.Orange, "===== Dedicated Server Patched =====");
    }

    public static unsafe void PatchMaxPlayerLimit()
    {
        var address = Reflow.GetInMethod<ServerDedicatedConfig>(
            nameof(ServerDedicatedConfig.Validate), 
            "\x8B\x40\x28\x83\xF8\x08".ToCharArray(),
            "?x?xxx".ToCharArray(), 3);

        if(address == 0)
            throw new Exception("Failed to find patch address");
        
        RLog.Msg(SysColor.Pink, $"Player Limit Address: {address.ToString("X")}");

        Reflow.NopMemory(address, 9);
    }

    private static void LaunchStartPatch(SonsLaunch __instance)
    {
        RLog.Msg("===== Launch Start! =====");
    }

    private static void BeforeServerCreation(DedicatedServerBoostrap __instance)
    {
        
    }

    private static void AlterServerName(ref string pszServerName)
    {
        pszServerName = $"<color=red>[MODDED]</color> {pszServerName}";
    }
    
    private static void LogPatch(Object message) => RLog.Msg(message.ToString());
    private static void LogWarningPatch(Object message) => RLog.Warning(message.ToString());
    private static void LogErrorPatch(Object message) => RLog.Error(message.ToString());
}

// [HarmonyPatch(typeof(CoopServerCallbacks), nameof(CoopServerCallbacks.Connected))]
// public static class ConnectedPatch
// {
//     public static void Prefix(CoopServerCallbacks __instance, BoltConnection connection)
//     {
//         try
//         { connection.SetCanReceiveEntities(false);
//         
//             if (GameServerManager.IsDedicatedServer)
//             {
//                 connection.Disconnect(new RedToken()
//                 {
//                     Banned = false,
//                     KickMessage = "Something happened."
//                 }.Cast<IProtocolToken>());
//                 return;
//             }
//             RLog.Msg(System.Drawing.Color.Orange, $"CoopServerCallbacks.Connected({connection})"); }
//         catch (Exception e)
//         {
//             Console.WriteLine(e);
//         }
//     }
// }
//
// public class RedToken : Object
// {
//     public bool Banned;
//
//     public string KickMessage;
//     
//     public RedToken(IntPtr ptr) : base(ptr) { }
//     public RedToken() : base(ClassInjector.DerivedConstructorPointer<RedToken>())
//     {
//         ClassInjector.DerivedConstructorBody(this);
//     }
//
//     public void Write(UdpPacket packet)
//     {
//         packet.WriteBool(Banned);
//         packet.WriteString(KickMessage);
//     }
//
//     public void Read(UdpPacket packet)
//     {
//         Banned = packet.ReadBool();
//         KickMessage = packet.ReadString();
//     }
// }
