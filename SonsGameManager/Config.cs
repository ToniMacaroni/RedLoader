using Harmony;
using MelonLoader;
using SonsSdk;
using TheForest;
using UnityEngine;
using Object = System.Object;

namespace SonsGameManager;

public static class Config
{
    internal static bool ShouldLoadIntoMain;

    public static ConfigCategory CoreCategory { get; private set; }
    public static ConfigCategory TestWorldLoaderCategory { get; private set; }
    public static ConfigCategory GameTweaksCategory { get; private set; }
    
    // Core
    public static ConfigEntry<bool> RedirectDebugLogs { get; private set; }
    public static ConfigEntry<bool> SkipIntro { get; private set; }

    // Test World Loader
    public static ConfigEntry<bool> DontAutoAddScenes { get; private set; }
    public static ConfigEntry<bool> DontLoadSaves { get; private set; }
    public static ConfigEntry<bool> ActivateWorldObjects { get; private set; }
    public static ConfigEntry<float> PlayerDebugSpeed { get; private set; }
    
    // Game Tweaks
    public static ConfigEntry<bool> SkipBuildingAnimations { get; private set; }

    public static void Load()
    {
        CoreCategory = ConfigSystem.GetCategory("core");
        TestWorldLoaderCategory = ConfigSystem.CreateCategory("test_world_loader", "Test World Loader");
        GameTweaksCategory = ConfigSystem.CreateCategory("game_tweaks", "Game Tweaks");

        // Core
        RedirectDebugLogs = CoreCategory.CreateEntry(
            "redirect_debug_logs", 
            false, 
            "Redirect Debug Logs", 
            "Redirect Debug Logs of the game to the console.");
        
        SkipIntro = CoreCategory.CreateEntry(
            "skip_intro",
            true,
            "Skip Intro",
            "Skip the EndNight intro.");
        
        // Test World Loader
        DontAutoAddScenes = TestWorldLoaderCategory.CreateEntry(
            "dont_auto_add_scenes",
            false,
            "Don't Auto Add Scenes",
            "Don't add additional scenes automatically. Lakes and camp items will not be added.");
        
        DontLoadSaves = TestWorldLoaderCategory.CreateEntry(
            "dont_load_saves",
            false,
            "Don't Load Saves",
            "Stops the whole game activation process. No game mechanics will work.");
        
        ActivateWorldObjects = TestWorldLoaderCategory.CreateEntry(
            "activate_world_objects",
            true,
            "Activate World Objects",
            "Doesn't load trees, bushes, rocks, etc. when disabled.");
        
        PlayerDebugSpeed = TestWorldLoaderCategory.CreateEntry(
            "player_debug_speed",
            1f,
            "Player Debug Speed",
            "A multiplier for the speed of the player in the test world.");
        
        // Game Tweaks
        SkipBuildingAnimations = GameTweaksCategory.CreateEntry(
            "skip_building_animations",
            false,
            "Skip Building Animations",
            "Skip the building animations.");
        
        ShouldLoadIntoMain = MelonLaunchOptions.SonsSdk.LoadIntoMain;
    }
}