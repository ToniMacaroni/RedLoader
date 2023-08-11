using AdvancedTerrainGrass;
using Endnight.Editor;
using Harmony;
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

        if (Config.RedirectDebugLogs.Value)
        {
            _patcher.Prefix<Debug>(nameof(Debug.Log), nameof(LogPatch), typeof(Object));
        }

        if (Config.ShouldLoadIntoMain)
        {
            AutoAddScene.DisableAll();
            _patcher.Prefix<LoadSave>(nameof(LoadSave.Awake), nameof(LoadSavePatch));
            _patcher.Prefix<LoadSave>(nameof(LoadSave.Start), nameof(LoadSavePatch));
            _patcher.Prefix<WorldObjectLocatorManager>(nameof(WorldObjectLocatorManager.OnEnable), nameof(WorldActivatorPatch));
        }
    }
    
    private static void LaunchStartPatch(SonsLaunch __instance)
    {
        Core.Log("===== Launch Start! =====");
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

        return eventPath is not (
            "event:/SotF_Music/mx_endnightsting" or 
            "event:/SotF_Music/Main Theme/Main_Theme"
            );
    }
    
    private static void LogPatch(Object message)
    {
        Core.Logger.Msg(message.ToString());
    }

    private static bool LoadSavePatch()
    {
        Core.Log("Stopped LoadSave");
        return false;
    }

    private static bool WorldActivatorPatch()
    {
        Core.Log("Stopped WorldObject activation");
        return false;
    }
}