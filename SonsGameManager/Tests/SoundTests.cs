using FMODCustom;
using RedLoader;
using RedLoader.Utils;
using SonsSdk;
using TheForest;
using TheForest.Utils;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Color = System.Drawing.Color;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace SonsGameManager;

public class SoundTests
{
    private static void Log(string msg, params object[] additional)
        => RLog.Msg(Color.Pink, msg + (additional.Length>0?" ":"") + string.Join(" ", additional));

    public static void Init()
    {
        SdkEvents.OnGameStart.Subscribe(OnGameStart);
        
        var path = @"C:\Users\Julian\Downloads\DDOI.mp3";
        var sound = SoundTools.RegisterSound("ddoi", path, true);
        sound?.set3DMinMaxDistance(1, 5);
        Log("Registered sound!");

        SoundTools.LoadBank(@"C:\Users\Julian\Documents\FMOD Studio\sotf\Build\Desktop\TheBang.bank");
        var evnt = FMOD_StudioSystem._instance.GetEvent("event:/Skyler");
        Assert.True(evnt.isValid());
        //evnt.start();

        // var emitter = Camera.main.gameObject.AddComponent<FMOD_StudioEventEmitter>();
        // emitter.SetEventPath("event:/Skyler");
        // emitter.Play();

        RLog.DebugBig("Bank Loaded");
        
        //SoundTools.SetupRedirect("event:/SotF Events/Ambient/Water/waves_shore", "event:/Skyler");
    }

    private static void OnGameStart()
    {
        DebugConsole.RegisterCommand("fmodist", (Il2CppSystem.Func<string, bool>)SetDistance, DebugConsole.Instance);
    }

    private static bool SetDistance(string args)
    {
        var parts = args.Split(' ').Select(float.Parse).ToArray();
        var min = parts[0];
        var max = parts[1];

        Player.ChannelDistance = (min, max);
        RLog.Debug($"Distances {min} {max}");
        return true;
    }

    private static GameObject GetObjectToAttachTo()
    {
        var go = new GameObject("Sound Player");
        var player = go.AddComponent<SoundPlayer>();
        player.Sound = SoundTools.GetSound("mysound");
        go.transform.position = LocalPlayer.Transform.position;
        return go;
    }

    public static SoundPlayer Player { get; set; }
}