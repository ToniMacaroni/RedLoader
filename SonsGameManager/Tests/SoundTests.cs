using FMOD;
using RedLoader;
using SonsSdk;
using TheForest;
using TheForest.Utils;
using UnityEngine;
using Color = System.Drawing.Color;

namespace SonsGameManager;

public class SoundTests
{
    private static void Log(string msg, params object[] additional)
        => RLog.Msg(Color.Pink, msg + (additional.Length>0?" ":"") + string.Join(" ", additional));

    public static void Init()
    {
        GlobalEvents.OnUpdate.Subscribe(OnUpdate);
        SdkEvents.OnGameStart.Subscribe(OnGameStart);
        
        var path = @"C:\Users\Julian\Downloads\DDOI.mp3";
        var sound = SoundTools.RegisterSound("ddoi", path, true);
        sound?.set3DMinMaxDistance(1, 5);
        Log("Registered sound!");
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
    

    private static void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            var go = GetObjectToAttachTo();
            Player = SoundTools.BindSound(go, "ddoi");
            Player.Play();
            Log("Playing sound");
        }
    }

    private static GameObject GetObjectToAttachTo()
    {
        var go = new GameObject("Sound Player");
        go.transform.position = LocalPlayer.Transform.position;
        return go;
    }

    public static SoundPlayer Player { get; set; }
}