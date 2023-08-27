using FMOD;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using RedLoader;
using Sons.Settings;
using UnityEngine;

namespace SonsSdk;

public static class SoundTools
{
    private static readonly Dictionary<string, Sound> Sounds = new();

    private static FMOD_StudioSystem System
    {
        get
        {
            FMOD_StudioSystem.TryGetInstance(out var system);
            return system;
        }
    }

    private static readonly Lazy<FMOD.System> CoreSystem = new(() =>
    {
        System.System.getCoreSystem(out var coreSystem);
        return coreSystem;
    });
    
    private static readonly Lazy<ChannelGroup> ChannelGroup = new(() =>
    {
        CoreSystem.Value.getMasterChannelGroup(out var channelGroup);
        return channelGroup;
    });

    /// <summary>
    /// The master volume in the game settings.
    /// </summary>
    public static float MasterVolume => AudioSettings._masterVolume;
    
    /// <summary>
    /// The music volume in the game settings. Multiplied by the master volume.
    /// </summary>
    public static float MusicVolume => AudioSettings._musicVolume * AudioSettings._masterVolume;
    
    /// <summary>
    /// The sfx volume in the game settings. Multiplied by the master volume.
    /// </summary>
    public static float SfxVolume => AudioSettings._sfxVolume * AudioSettings._masterVolume;
    
    /// <summary>
    /// The voice volume in the game settings. Multiplied by the master volume.
    /// </summary>
    public static float VoiceVolume => AudioSettings._voiceVolume * AudioSettings._masterVolume;

    /// <summary>
    /// Register a sound to the fmod system from a file. You can play it with <see cref="PlaySound"/>.
    /// </summary>
    /// <param name="id">The id of the sound by which you can play it later</param>
    /// <param name="filepath">The file path of the sound</param>
    public static void RegisterSound(string id, string filepath, bool use3d = false)
    {
        if (Sounds.ContainsKey(id))
        {
            RLog.Error($"Sound with id {id} already registered");
            return;
        }
        
        CoreSystem.Value.createSound(filepath, use3d ? MODE._3D : MODE.DEFAULT, out var sound);
        
        Sounds.Add(id, sound);
    }

    /// <summary>
    /// Register a sound to the fmod system from memory. You can play it with <see cref="PlaySound"/>.
    /// </summary>
    /// <param name="id">The id of the sound by which you can play it later</param>
    /// <param name="data">the data of the sound file</param>
    public static void RegisterSound(string id, byte[] data, bool use3d = false)
    {
        if (Sounds.ContainsKey(id))
        {
            RLog.Error($"Sound with id {id} already registered");
            return;
        }

        var info = new CREATESOUNDEXINFO();

        CoreSystem.Value.createSound(data, use3d ? MODE._3D : MODE.DEFAULT, ref info, out var sound);
        
        Sounds.Add(id, sound);
    }
    
    /// <summary>
    /// Play a registered sound
    /// </summary>
    /// <param name="id">The id you specified in <see cref="RegisterSound"/></param>
    /// <param name="volume">The volume of the sound. If nothing is specified the settings music volume is used</param>
    /// <param name="pitch">The pitch of the sound. 1 is normal pitch</param>
    /// <returns>A channel which let's you control and stop the sound again</returns>
    public static Channel? PlaySound(string id, float? volume = null, float? pitch = null)
    {
        if(!Sounds.TryGetValue(id, out var sound))
        {
            RLog.Error($"Sound with id {id} not registered");
            return null;
        }

        volume ??= AudioSettings._musicVolume * AudioSettings._masterVolume;
        
        CoreSystem.Value.playSound(sound, ChannelGroup.Value, false, out var ch);

        ch.setVolume(volume.Value);

        if (pitch.HasValue)
            ch.setPitch(pitch.Value);
        
        return ch;
    }

    /// <summary>
    /// Play a registered sound
    /// </summary>
    /// <param name="id">The id you specified in <see cref="RegisterSound"/></param>
    /// <param name="pos">The position at which to play the sound</param>
    /// <param name="volume">The volume of the sound. If nothing is specified the settings music volume is used</param>
    /// <param name="pitch">The pitch of the sound. 1 is normal pitch</param>
    /// <returns>A channel which let's you control and stop the sound again</returns>
    public static Channel? PlaySound(string id, Vector3 pos, float? volume = null, float? pitch = null)
    {
        if(!Sounds.TryGetValue(id, out var sound))
        {
            RLog.Error($"Sound with id {id} not registered");
            return null;
        }

        sound.getMode(out var mode);
        var is3d = mode.HasFlag(MODE._3D);
        
        if(!is3d)
            throw new Exception("Trying to play a non 3d sound with a position");

        volume ??= AudioSettings._musicVolume * AudioSettings._masterVolume;
        
        CoreSystem.Value.playSound(sound, ChannelGroup.Value, true, out var ch);
        
        var vec = new VECTOR
        {
            x = pos.x,
            y = pos.y,
            z = pos.z
        };
        
        var vel = new VECTOR
        {
            x = 0,
            y = 0,
            z = 0
        };

        ch.set3DAttributes(ref vec, ref vel);

        ch.setVolume(volume.Value);

        if (pitch.HasValue)
            ch.setPitch(pitch.Value);

        ch.setPaused(false);
        
        return ch;
    }
}