﻿using Harmony;
using RedLoader;
using SonsSdk;
using SonsSdk.Attributes;
using TheForest;
using TheForest.Utils;
using TMPro;
using UnityEngine;
using Object = System.Object;

namespace SonsGameManager;

[SettingsUiMode(SettingsUiMode.ESettingsUiMode.OptIn)]
public static class Config
{
    internal static bool ShouldLoadIntoMain;
    internal static string LoadSaveGame;

    public static ConfigCategory CoreCategory { get; private set; }
    public static ConfigCategory TestWorldLoaderCategory { get; private set; }
    public static ConfigCategory GameTweaksCategory { get; private set; }
    public static ConfigCategory FreeCamCategory { get; private set; }
    
    // ================ Core ================
    
    /// <summary>
    /// Redirect Debug Logs of the game to the console.
    /// </summary>
    /// <config>Redirect Debug Logs</config>
    /// <type>bool</type>
    public static ConfigEntry<bool> RedirectDebugLogs { get; private set; }
    
    /// <summary>
    /// Skip the EndNight intro.
    /// </summary>
    /// <config>Skip Intro</config>
    /// <type>bool</type>
    [SettingsUiInclude]
    public static ConfigEntry<bool> SkipIntro { get; private set; }
    
    
    // ================ Core ================
    
    /// <summary>
    /// List of sounds that should be muted.
    /// </summary>
    /// <config>Muted Sounds</config>
    /// <type>List[string]</type>
    public static ConfigEntry<List<string>> MutedSounds { get; private set; }

    /// <summary>
    /// Key used to toggle the in-game console.
    /// </summary>
    /// <config>Toggle Console Key</config>
    /// <type>KeyCode</type>
    public static ConfigEntry<KeyCode> ToggleConsoleKey { get; private set; }

    // ================ Test World Loader ================

    /// <summary>
    /// Indicates whether additional scenes should not be added automatically.
    /// </summary>
    /// <config>Don't Auto Add Scenes</config>
    /// <type>bool</type>
    public static ConfigEntry<bool> DontAutoAddScenes { get; private set; }

    /// <summary>
    /// Indicates whether the game should skip the activation process and not load saves.
    /// </summary>
    /// <config>Don't Load Saves</config>
    /// <type>bool</type>
    public static ConfigEntry<bool> DontLoadSaves { get; private set; }

    /// <summary>
    /// Indicates whether world objects such as trees, bushes, and rocks should be activated or not.
    /// </summary>
    /// <config>Activate World Objects</config>
    /// <type>bool</type>
    public static ConfigEntry<bool> ActivateWorldObjects { get; private set; }

    /// <summary>
    /// Multiplier for the speed of the player in the test world.
    /// </summary>
    /// <config>Player Debug Speed</config>
    /// <type>float</type>
    public static ConfigEntry<float> PlayerDebugSpeed { get; private set; }

    // ================ Game Tweaks ================

    /// <summary>
    /// Indicates whether building animations should be skipped.
    /// </summary>
    /// <config>Skip Building Animations</config>
    /// <type>bool</type>
    [SettingsUiInclude]
    public static ConfigEntry<bool> SkipBuildingAnimations { get; private set; }

    /// <summary>
    /// Indicates whether the bow trajectory should be displayed when aiming.
    /// </summary>
    /// <config>Enable Bow Trajectory</config>
    /// <type>bool</type>
    public static ConfigEntry<bool> EnableBowTrajectory { get; private set; }
    
    /// <summary>
    /// Whether to not play any animations when consuming items.
    /// </summary>
    /// <config>No Consume Animation</config>
    /// <type>bool</type>
    [SettingsUiInclude]
    public static ConfigEntry<bool> NoConsumeAnimation { get; private set; }
    
    /// <summary>
    /// Don't automatically equip stones when picking them up.
    /// </summary>
    /// <config>No Auto Equip Stones</config>
    /// <type>bool</type>
    [SettingsUiInclude]
    public static ConfigEntry<bool> NoAutoEquipStones { get; private set; }
    
    /// <summary>
    /// Instantly open the inventory without animations.
    /// </summary>
    /// <config>Instant Inventory Open</config>
    /// <type>bool</type>
    [SettingsUiInclude]
    public static ConfigEntry<bool> InstantInventoryOpen { get; private set; }

    // ================ Free Cam ================
    [SettingsUiHeader("These settings affect the 'xfreecam' command")]
    [SettingsUiInclude]
    public static ConfigEntry<float> LookSensitivty { get; private set; }
    
    [SettingsUiInclude]
    public static ConfigEntry<float> PositionalSmoothing { get; private set; }
    
    [SettingsUiInclude]
    public static ConfigEntry<float> RotationalSmoothing { get; private set; }
    
    [SettingsUiInclude]
    public static ConfigEntry<float> MouseYRatio { get; private set; }
    
    
    public static HashSet<string> SavedMutesSounds;

    public static void Load()
    {
        CoreCategory = ConfigSystem.GetCategory("core");
        TestWorldLoaderCategory = ConfigSystem.CreateCategory("test_world_loader", "Test World Loader");
        GameTweaksCategory = ConfigSystem.CreateCategory("game_tweaks", "Game Tweaks");
        FreeCamCategory = ConfigSystem.CreateCategory("free_cam", "Free Cam");

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

        MutedSounds = CoreCategory.CreateEntry(
            "muted_sounds",
            new List<string>
            {
                // "event:/SotF_Music/mx_endnightsting",
                // "event:/SotF_Music/Main Theme/Main_Theme",
                // "event:/SotF_Music/Main Theme/Main_Theme_helicopter",
            },
            "Muted Sounds",
            "Sounds that are muted.");
        
        ToggleConsoleKey = CoreCategory.CreateEntry(
            "toggle_console_key",
            KeyCode.Pause,
            "Toggle Console Key",
            "The key that toggles the console.");

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
        
        NoConsumeAnimation = GameTweaksCategory.CreateEntry(
            "no_consume_animation",
            false,
            "No Consume Animation",
            "Don't play any animations when consuming items.");
        
        NoAutoEquipStones = GameTweaksCategory.CreateEntry(
            "no_auto_equip_stones",
            false,
            "No Auto Equip Stones",
            "Don't automatically equip stones when they are picked up.");
        
        InstantInventoryOpen = GameTweaksCategory.CreateEntry(
            "instant_inventory_open",
            false,
            "Instant Inventory Open",
            "Instantly open the inventory without animations.");
        
        // EnableBowTrajectory = GameTweaksCategory.CreateEntry(
        //     "enable_bow_trajectory",
        //     false,
        //     "Enable Bow Trajectory",
        //     "Show the bow trajectory when aiming.");
        
        // Free Cam
        LookSensitivty = FreeCamCategory.CreateEntry(
            "look_sensitivity",
            0.2f,
            "Look Sensitivity",
            "The sensitivity of the mouse when looking around.");
        
        PositionalSmoothing = FreeCamCategory.CreateEntry(
            "positional_smoothing",
            0.5f,
            "Positional Smoothing",
            "The amount of smoothing applied to the camera's position.");
        
        RotationalSmoothing = FreeCamCategory.CreateEntry(
            "rotational_smoothing",
            0.01f,
            "Rotational Smoothing",
            "The amount of smoothing applied to the camera's rotation.");
        
        MouseYRatio = FreeCamCategory.CreateEntry(
            "mouse_y_ratio",
            0.7f,
            "Mouse Y Ratio",
            "The ratio of the mouse's Y speed to the X axis.");

        SavedMutesSounds = MutedSounds.Value.ToHashSet();
        
        ShouldLoadIntoMain = LaunchOptions.SonsSdk.LoadIntoMain;
        LoadSaveGame = LaunchOptions.SonsSdk.LoadSaveGame;
    }

    public static void OnSettingsUiClosed()
    {
        if (LocalPlayer._instance)
        {
            LocalPlayer.Inventory._inventoryCutscene._inventoryBagAnimator.speed =
                InstantInventoryOpen.Value ? 200 : 1;
            LocalPlayer.Inventory._inventoryCutscene._layoutGroupsRolloutAnimator.speed =
                InstantInventoryOpen.Value ? 200 : 1;
        }
    }
}