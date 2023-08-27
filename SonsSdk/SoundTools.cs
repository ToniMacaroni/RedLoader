using FMOD;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using RedLoader;
using Sons.Settings;
using UnityEngine;
using Color = System.Drawing.Color;

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
    public static Sound? RegisterSound(string id, string filepath, bool use3d = false)
    {
        if (Sounds.ContainsKey(id))
        {
            RLog.Error($"Sound with id {id} already registered");
            return null;
        }
        
        CoreSystem.Value.createSound(filepath, use3d ? MODE._3D : MODE.DEFAULT, out var sound);
        
        Sounds.Add(id, sound);

        return sound;
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
    /// <param name="maxDist">The maximum distance at which the sound is still audible</param>
    /// <param name="volume">The volume of the sound. If nothing is specified the settings music volume is used</param>
    /// <param name="pitch">The pitch of the sound. 1 is normal pitch</param>
    /// <returns>A channel which let's you control and stop the sound again</returns>
    public static Channel? PlaySound(string id, Vector3 pos, float? maxDist = null, float? volume = null, float? pitch = null)
    {
        if(!Sounds.TryGetValue(id, out var sound))
        {
            throw new Exception($"Sound with id {id} not registered");
        }


        return PlaySound(sound, pos, maxDist, volume, pitch);
    }

    public static Channel? PlaySound(Sound sound, Vector3 pos, float? maxDist = null, float? volume = null, float? pitch = null)
    {
        sound.getMode(out var mode);
        var is3d = mode.HasFlag(MODE._3D);
        
        if(!is3d)
            throw new Exception("Trying to play a non 3d sound with a position");

        CoreSystem.Value.playSound(sound, ChannelGroup.Value, true, out var ch);
        
        SetPosition(ref ch, pos.x, pos.y, pos.z);
        ch.setMode(MODE._3D_LINEARROLLOFF);

        volume ??= AudioSettings._musicVolume * AudioSettings._masterVolume;

        ch.setVolume(volume.Value);

        if (pitch.HasValue)
            ch.setPitch(pitch.Value);
        
        if(maxDist.HasValue)
            ch.set3DMinMaxDistance(1, maxDist.Value);

        ch.setPaused(false);
        
        return ch;
    }
    
    /// <summary>
    /// Gets the sound by id.
    /// </summary>
    /// <param name="sound"></param>
    /// <param name="volume"></param>
    /// <param name="pitch"></param>
    /// <returns></returns>
    public static Channel? PlaySound(Sound sound, float volume, float? pitch = null)
    {
        CoreSystem.Value.playSound(sound, ChannelGroup.Value, false, out var ch);
        
        ch.setVolume(volume);

        if (pitch.HasValue)
            ch.setPitch(pitch.Value);

        return ch;
    }
    
    public static Sound? GetSound(string id)
    {
        if(!Sounds.TryGetValue(id, out var sound))
        {
            RLog.Error($"Sound with id {id} not registered");
            return null;
        }

        return sound;
    }

    public static void SetPosition(ref Channel channel, float x, float y, float z)
    {
        var vec = new VECTOR
        {
            x = x,
            y = y,
            z = z
        };
        
        var vel = new VECTOR
        {
            x = 0,
            y = 0,
            z = 0
        };

        channel.set3DAttributes(ref vec, ref vel);
    }

    /// <summary>
    /// Bind a sound to a gameobject. The sound will be played at the position of the gameobject.
    /// </summary>
    /// <param name="go">The gameobject to bind the sound to</param>
    /// <param name="id">The id of the sound to play</param>
    /// <returns></returns>
    public static SoundPlayer BindSound(GameObject go, string id)
    {
        var sound = GetSound(id);
        if(!sound.HasValue)
            return null;
        
        var player = go.AddComponent<SoundPlayer>();
        player.Sound = sound.Value;
        return player;
    }
}

public class SoundPlayer : MonoBehaviour
{
    public Sound Sound;

    private Channel? _channel;
    private Transform t;
    
    public Channel? Channel => _channel;

    public (float min, float max) ChannelDistance
    {
        get
        {
            if (!_channel.HasValue)
                return (0, 0);
            
            float min, max;
            _channel.Value.get3DMinMaxDistance(out min, out max);
            return (min, max);
        }
        set
        {
            if(!_channel.HasValue)
                return;
            
            _channel.Value.set3DMinMaxDistance(value.min, value.max);
        }
    }

    public float? MaxDistance;

    public bool IsPlaying
    {
        get
        {
            if (!_channel.HasValue)
                return false;

            _channel.Value.isPlaying(out var isplaying);
            return isplaying;
        }
    }

    static SoundPlayer()
    {
        ClassInjector.RegisterTypeInIl2Cpp<SoundPlayer>();
    }

    private void Awake()
    {
        t = transform;
    }

    private void Update()
    {
        if (!IsPlaying)
            return;

        var ch = _channel!.Value;
        var pos = t.position;
        
        SoundTools.SetPosition(ref ch, pos.x, pos.y, pos.z);
    }

    public Channel? Play()
    {
        Stop();
        
        _channel = SoundTools.PlaySound(Sound, t.position, MaxDistance);
        return _channel;
    }
    
    public void Stop()
    {
        if (!IsPlaying)
            return;

        _channel!.Value.stop();
        _channel = null;
    }
}