using AdvancedTerrainGrass;
using Endnight.Editor;
using Harmony;
using RedLoader;
using Sons.Multiplayer.Dedicated;
using Sons.TerrainDetail;
using TheForest;
using UnityEngine;
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
        _patcher.Prefix<SonsFMODEventEmitter>(nameof(SonsFMODEventEmitter.Play), nameof(FModPatch));
        _patcher.Prefix<FMOD_StudioEventEmitter>(nameof(FMOD_StudioEventEmitter.Play), nameof(FModEmitterPatch));

        if (Config.RedirectDebugLogs.Value)
        {
            _patcher.Prefix<Debug>(nameof(Debug.Log), nameof(LogPatch), typeof(Object));
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
    }
    
    private static void LaunchStartPatch(SonsLaunch __instance)
    {
        RLog.Msg("===== Launch Start! =====");
        GameBootLogoPatch.CreateBlackScreen();
        if (Config.SkipIntro.Value)
        {
            __instance._titleSceneLoader._delay = 0f;
        }
        else
        {
            GameBootLogoPatch.GlobalOverlay.SetActive(false);
        }
    }
    
    private static bool FModPatch(SonsFMODEventEmitter __instance)
    {
        var eventPath = __instance._eventPath;

        return !Config.SavedMutesSounds.Contains(eventPath);
    }
    
    private static bool FModEmitterPatch(FMOD_StudioEventEmitter __instance)
    {
        var eventPath = __instance._eventPath;
        RLog.Msg("FModEmitterPatch: " + eventPath);
        __instance.Play();
        return true;
        //return !Config.SavedMutesSounds.Contains(eventPath);
    }
    
    private static void LogPatch(Object message)
    {
        RLog.Msg(message.ToString());
    }

    private static bool LoadSavePatch()
    {
        RLog.Msg("Stopped LoadSave");
        return false;
    }

    private static bool WorldActivatorPatch()
    {
        RLog.Msg("Stopped WorldObject activation");
        return false;
    }
}