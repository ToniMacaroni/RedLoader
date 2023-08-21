using FMOD;
using RedLoader;
using UnityEngine.SceneManagement;

namespace SonsSdk;

public static class SoundTools
{
    private static readonly Dictionary<string, Sound> Sounds = new();
    private static readonly Lazy<FMOD.System> System = new(() =>
    {
        FMOD_StudioSystem.TryGetInstance(out var system);
        system.System.getCoreSystem(out var coreSystem);
        return coreSystem;
    });
    
    private static readonly Lazy<ChannelGroup> ChannelGroup = new(() =>
    {
        System.Value.getMasterChannelGroup(out var channelGroup);
        return channelGroup;
    });

    public static void RegisterSound(string id, string filepath)
    {
        if (Sounds.ContainsKey(id))
        {
            RLog.Error($"Sound with id {id} already registered");
            return;
        }
        
        System.Value.createSound(filepath, MODE.DEFAULT, out var sound);
        
        Sounds.Add(id, sound);
    }
    
    public static Channel? PlaySound(string id, float? volume = null, float? pitch = null)
    {
        if(!Sounds.TryGetValue(id, out var sound))
        {
            RLog.Error($"Sound with id {id} not registered");
            return null;
        }
        
        System.Value.playSound(sound, ChannelGroup.Value, false, out var ch);
        
        if (volume.HasValue)
            ch.setVolume(volume.Value);

        if (pitch.HasValue)
            ch.setPitch(pitch.Value);
        
        return ch;
    }
}