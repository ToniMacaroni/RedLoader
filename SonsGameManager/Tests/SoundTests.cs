using RedLoader;
using SonsSdk;
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
        
        var path = @"C:\Users\Julian\Downloads\DDOI.mp3";
        SoundTools.RegisterSound("ddoi", path, true);
        Log("Registered sound!");
    }

    private static void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            var go = ActorTools.GetRobby().gameObject;
            SoundTools.BindSound(go, "ddoi").Play();
            Log("Playing sound on Robby!");
        }
    }
}