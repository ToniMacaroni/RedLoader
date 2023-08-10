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

        if (GameCore.RedirectDebugLogs)
        {
            _patcher.Prefix<Debug>(nameof(Debug.Log), nameof(LogPatch), typeof(Object));
        }

        if (GameCore.ShouldLoadIntoMain)
        {
            AutoAddScene.DisableAll();
            _patcher.Prefix<LoadSave>(nameof(LoadSave.Awake), nameof(LoadSavePatch));
            _patcher.Prefix<LoadSave>(nameof(LoadSave.Start), nameof(LoadSavePatch));
            _patcher.Prefix<WorldObjectLocatorManager>(nameof(WorldObjectLocatorManager.OnEnable), nameof(WorldActivatorPatch));
            _patcher.Prefix<GrassManager>(nameof(GrassManager.RefreshGrassRenderingSettings), nameof(GrassManagerPatch));
        }
    }
    
    private static void LaunchStartPatch(SonsLaunch __instance)
    {
        Core.Log("===== Launch Start! =====");
        GameBootLogoPatch.CreateBlackScreen();
        if (GameCore.SkipIntro)
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
    
    private static void GrassManagerPatch(GrassManager __instance, ref float t_DetailDensity, ref float t_CullDistance, ref float t_FadeLength, ref float t_CacheDistance, ref float t_SmallDetailFadeStart, ref float t_SmallDetailFadeLength, ref float t_DetailFadeStart, ref float t_DetailFadeLength, ref float t_ShadowStart, ref float t_ShadowFadeLength, ref float t_ShadowStartFoliage, ref float t_ShadowFadeLengthFoliage, ref bool t_UseLodMesh)
    {
        t_DetailDensity *= 100;
        t_CullDistance *= 100;
        Core.Log("Updating GrassManager");
    }
}