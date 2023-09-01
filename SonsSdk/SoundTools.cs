using System.Runtime.InteropServices;
using FMOD.Studio;
using Il2cppFmod = FMOD;
using FMODCustom;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using RedLoader;
using Sons.Settings;
using UnityEngine;
using Color = System.Drawing.Color;

namespace SonsSdk;

public static class SoundTools
{
    private static readonly Dictionary<string, Sound> Sounds = new();
    internal static readonly Dictionary<string, string> EventRedirects = new();

    public static Lazy<FMODCustom.System> CoreSystem = new(() =>
    {
        FMOD_StudioSystem._instance.System.getCoreSystem(out var core);
        return new FMODCustom.System(core.handle);
    });

    private static FMOD_StudioSystem System
    {
        get
        {
            FMOD_StudioSystem.TryGetInstance(out var system);
            return system;
        }
    }

    private static readonly Lazy<ChannelGroup> ChannelGroup = new(() =>
    {
        CoreSystem.Value.getMasterChannelGroup(out var channelGroup);
        return channelGroup;
    });

    public static Bus MasterBus
    {
        get
        {
            FMOD_StudioSystem._instance.System.getBus("bus:/", out var bus);
            return bus;
        }
    }

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
    public static Sound RegisterSound(string id, string filepath, bool use3d = false)
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

        CREATESOUNDEXINFO info = new CREATESOUNDEXINFO
        {
            cbsize = Marshal.SizeOf(typeof(CREATESOUNDEXINFO)),
            length = (uint)data.Length
        };

        CoreSystem.Value.createSound(data, use3d ? MODE._3D : MODE.DEFAULT, ref info, out var sound);
        
        Sounds.Add(id, sound);
    }

    /// <summary>
    /// Loads and registeres a bank file
    /// </summary>
    /// <param name="path">The path of the bank file (Make sure the {name}.strings.bank file is also present at that location)</param>
    /// <param name="loadStringsFile">If the .strings.bank file should also be loaded. If true make sure it exists beside the .bank file</param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public static Bank? LoadBank(string path, bool loadStringsFile = true, LOAD_BANK_FLAGS flags = LOAD_BANK_FLAGS.NORMAL)
    {
        if (loadStringsFile)
        {
            var stringsPath = path.Replace(".bank", ".strings.bank");
            if (!File.Exists(stringsPath))
            {
                RLog.Error($"Strings file {stringsPath} not found");
                return null;
            }
        
            var stringsResult = System.System.loadBankFile(stringsPath, flags, out var stringsBank);
        
            if (stringsResult != Il2cppFmod.RESULT.OK)
            {
                RLog.Error($"Failed to load strings bank from file: {stringsResult}");
                return null;
            }
        }
        
        var result = System.System.loadBankFile(path, flags, out var bank);
        if (result != Il2cppFmod.RESULT.OK)
        {
            RLog.Error($"Failed to load bank from file: {result}");
            return null;
        }
        
        return bank;
    }

    /// <summary>
    /// Loads and registeres a bank from a byte buffer
    /// </summary>
    /// <param name="data">The .bank data</param>
    /// <param name="stringsData">The .strings.bank data</param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public static Bank? LoadBank(byte[] data, byte[] stringsData = null, LOAD_BANK_FLAGS flags = LOAD_BANK_FLAGS.NORMAL)
    {
        if (stringsData != null)
        {
            var stringsResult = System.System.loadBankMemory(stringsData, flags, out var stringsBank);
        
            if (stringsResult != Il2cppFmod.RESULT.OK)
            {
                RLog.Error($"Failed to load strings bank from memory: {stringsResult}");
                return null;
            }
        }
        
        var result = System.System.loadBankMemory(data, flags, out var bank);
        if (result != Il2cppFmod.RESULT.OK)
        {
            RLog.Error($"Failed to load bank from memory: {result}");
            return null;
        }
        
        return bank;
    }
    
    /// <summary>
    /// Play a registered sound
    /// </summary>
    /// <param name="id">The id you specified in <see cref="RegisterSound"/></param>
    /// <param name="volume">The volume of the sound. If nothing is specified the settings music volume is used</param>
    /// <param name="pitch">The pitch of the sound. 1 is normal pitch</param>
    /// <returns>A channel which let's you control and stop the sound again</returns>
    public static Channel PlaySound(string id, float? volume = null, float? pitch = null)
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
    public static Channel PlaySound(string id, Vector3 pos, float? maxDist = null, float? volume = null, float? pitch = null)
    {
        if(!Sounds.TryGetValue(id, out var sound))
        {
            throw new Exception($"Sound with id {id} not registered");
        }


        return PlaySound(sound, pos, maxDist, volume, pitch);
    }

    public static Channel PlaySound(Sound sound, Vector3 pos, float? maxDist = null, float? volume = null, float? pitch = null)
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
    public static Channel PlaySound(Sound sound, float volume, float? pitch = null)
    {
        CoreSystem.Value.playSound(sound, ChannelGroup.Value, false, out var ch);
        
        ch.setVolume(volume);

        if (pitch.HasValue)
            ch.setPitch(pitch.Value);

        return ch;
    }
    
    public static Sound GetSound(string id)
    {
        if(!Sounds.TryGetValue(id, out var sound))
        {
            RLog.Error($"Sound with id {id} not registered");
            return null;
        }

        return sound;
    }

    /// <summary>
    /// Redirects a registered fmod event to another event
    /// </summary>
    /// <param name="srcEvent">The original event</param>
    /// <param name="dstEvent">The event that should be played instead</param>
    public static void SetupRedirect(string srcEvent, string dstEvent)
    {
        if (EventRedirects.ContainsKey(srcEvent))
        {
            RLog.Error($"Event {srcEvent} already has a redirect");
        }

        EventRedirects[srcEvent] = dstEvent;
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

        var alt = new VECTOR();

        channel.set3DAttributes(ref vec, ref vel, ref alt);
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
        if(sound == null)
            return null;
        
        var player = go.AddComponent<SoundPlayer>();
        player.Sound = sound;
        return player;
    }

    public static class Debugging
    {
        public static Il2CppSystem.Guid GetBusId()
        {
            foreach (var loadedBanksValue in FMOD_StudioSystem._loadedBanks._values)
            {
                
            }
            MasterBus.getID(out var id);
            return id;
        }

        public static List<Bank> GetLoadedBanks()
        {
            var ret = new List<Bank>();
            
            foreach (var bank in FMOD_StudioSystem._loadedBanks._values)
            {
                ret.Add(bank);
            }
            
            return ret;
        }

        public static void PrintLoadedBanks()
        {
            foreach (var loadedBank in GetLoadedBanks())
            {
                loadedBank.getPath(out var path);
                Console.WriteLine(path);
            }
        }

        public static List<Bus> GetBusList(Bank bank)
        {
            var ret = new List<Bus>();
            
            bank.getBusList(out var busList);

            return ret;
        }
    }
}