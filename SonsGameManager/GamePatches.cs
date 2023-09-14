using System.Reflection;
using Endnight.Editor;
using Harmony;
using HarmonyLib;
using RedLoader;
using Sons.Gui;
using Sons.Multiplayer.Dedicated;
using Sons.Music;
using Sons.Save;
using Sons.TerrainDetail;
using SonsSdk;
using UnityEngine;
using Color = System.Drawing.Color;
using Object = Il2CppSystem.Object;

// ReSharper disable InconsistentNaming

namespace SonsGameManager;

public class GamePatches
{
    private static ConfiguredPatcher<GamePatches> _patcher;
    
    public static void Init()
    {
        _patcher = new(Core.HarmonyInst);
        
        _patcher.Prefix<SonsLaunch>("Start", nameof(LaunchStartPatch));
        _patcher.Prefix<SonsFMODEventEmitter>(nameof(SonsFMODEventEmitter.Play), nameof(SonsEmitterPlayPatch));
        _patcher.Prefix<FMOD_StudioEventEmitter>(nameof(FMOD_StudioEventEmitter.StartEvent), nameof(FModEmitterPlayPatch));
        
        var mt = typeof(MusicManager).GetMethod(nameof(MusicManager.SetMusicEvent), BindingFlags.Public | BindingFlags.Instance);
        var hm = new HarmonyMethod(typeof(GamePatches).GetMethod(nameof(MusicLookupPatch), BindingFlags.NonPublic | BindingFlags.Static));
        Core.HarmonyInst.Patch(mt, hm);

        if (Config.RedirectDebugLogs.Value)
        {
            _patcher.Prefix<Debug>(nameof(Debug.Log), nameof(LogPatch), true, typeof(Object));
            _patcher.Prefix<Debug>(nameof(Debug.LogWarning), nameof(LogWarningPatch), true, typeof(Object));
            _patcher.Prefix<Debug>(nameof(Debug.LogError), nameof(LogErrorPatch), true, typeof(Object));
        }

        if (Config.ShouldLoadIntoMain)
        {
            if(Config.DontAutoAddScenes.Value)
                AutoAddScene.DisableAll();
            
            if (Config.DontLoadSaves.Value)
            {
                _patcher.Prefix<LoadSave>(nameof(LoadSave.Awake), nameof(LoadSavePatch));
                _patcher.Prefix<LoadSave>(nameof(LoadSave.Start), nameof(LoadSavePatch));    
            }
            
            if(!Config.ActivateWorldObjects.Value)
                _patcher.Prefix<WorldObjectLocatorManager>(nameof(WorldObjectLocatorManager.OnEnable), nameof(WorldActivatorPatch));
        }
        
        _patcher.Prefix<PauseMenu>(nameof(PauseMenu.OnEnable), nameof(PauseMenuPatch));
    }
    
    private static void LaunchStartPatch(SonsLaunch __instance)
    {
        RLog.Msg("===== Launch Start! =====");
        LoadIntoMainHandler.CreateBlackScreen();
        if (Config.SkipIntro.Value)
        {
            __instance._titleSceneLoader._delay = 0f;
        }
        else
        {
            LoadIntoMainHandler.GlobalOverlay.SetActive(false);
        }
    }
    
    private static bool SonsEmitterPlayPatch(SonsFMODEventEmitter __instance)
    {
        var eventPath = __instance._eventPath;
        if(Core.LogSounds)
            RLog.Msg(Color.Green, "SonsEmitter sound: " + eventPath);
        if (SoundTools.EventRedirects.TryGetValue(eventPath, out var newPath))
        {
            __instance.SetEventPath(newPath);
            RLog.Debug($"\tRedirected to {newPath}");
        }
        return !Config.SavedMutesSounds.Contains(eventPath);
    }
    
    private static bool FModEmitterPlayPatch(FMOD_StudioEventEmitter __instance)
    {
        var eventPath = __instance._eventPath;
        if(Core.LogSounds)
            RLog.Msg(Color.Green, "FModEmitter sound: " + eventPath);
        if (SoundTools.EventRedirects.TryGetValue(eventPath, out var newPath))
        {
            __instance.SetEventPath(newPath);
            RLog.Debug($"\tRedirected to {newPath}");
        }
        __instance._forcedDisabled = Config.SavedMutesSounds.Contains(eventPath) || __instance._forcedDisabled;
        return true;
    }
    
    private static void MusicLookupPatch(MusicManager __instance, ref string eventPath)
    {
        RLog.Debug($"MusicManager.SetMusicEvent({eventPath})");
        if (SoundTools.EventRedirects.TryGetValue(eventPath, out var newPath))
        {
            eventPath = newPath;
            RLog.Debug($"\tRedirected to {newPath}");
        }
    }

    private static void LogPatch(Object message) => RLog.Msg(message.ToString());
    private static void LogWarningPatch(Object message) => RLog.Warning(message.ToString());
    private static void LogErrorPatch(Object message) => RLog.Error(message.ToString());
    
    private static void PauseMenuPatch(PauseMenu __instance)
    {
        RLog.DebugBig("PauseMenu created!");
        SUI.SUI.OnPauseMenuCreated(__instance);
    }

    private static bool LoadSavePatch()
    {
        RLog.Debug("Stopped LoadSave");
        return false;
    }

    private static bool WorldActivatorPatch()
    {
        RLog.Debug("Stopped WorldObject activation");
        return false;
    }
}

