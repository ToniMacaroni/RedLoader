global using SysColor = System.Drawing.Color;

using System.Diagnostics;
using System.Runtime.InteropServices;
using Construction;
using RedLoader;
using RedLoader.Utils;
using Sons.Gui;
using Sons.Save;
using SonsSdk;
using SUI;
using TheForest;
using TheForest.Utils;
using UnityEngine;

namespace SonsGameManager;

public partial class Core : SonsMod
{
    internal static Core Instance;

    internal static HarmonyLib.Harmony HarmonyInst => Instance.HarmonyInstance;

    public Core()
    {
        Instance = this;
    }

    protected override void OnInitializeMod()
    {
        Config.Load();
        if (!LoaderEnvironment.IsDedicatedServer)
        {
            GraphicsCustomizer.Load();
            GamePatches.Init();
            return;
        }
        
        ServerPatches.Init();
    }
    
    protected override void OnSdkInitialized()
    {
        if (LoaderEnvironment.IsDedicatedServer)
        {
            return;
        }
        
        ModManagerUi.Create();
        SettingsRegistry.CreateSettings(this, null, typeof(Config));
        
        if(Config.ShouldLoadIntoMain)
        {
            LoadIntoMainHandler.DelayedSceneLoad().RunCoro();
            return;
        }

        LoadIntoMainHandler.GlobalOverlay.SetActive(false);

        LoadTests();

        if (!string.IsNullOrEmpty(Config.LoadSaveGame) && int.TryParse(Config.LoadSaveGame, out var id))
        {
            Resources.FindObjectsOfTypeAll<LoadMenu>()
                .First(x=>x.name=="SinglePlayerLoadPanel")
                .LoadSlotActivated(id, SaveGameManager.GetData<GameStateData>(SaveGameType.SinglePlayer, id));
        }
    }

    [Conditional("DEBUG")]
    private void LoadTests()
    {
        //SuiTest.Create();
        //SoundTests.Init();
        //new DebugGizmoTest();
    }

    protected override void OnGameStart()
    {
        if (LoaderEnvironment.IsDedicatedServer)
        {
            return;
        }
        
        Log("======= GAME STARTED ========");

        // -- Enable debug console --
        DebugConsole.Instance.enabled = true;
        DebugConsole.SetCheatsAllowed(true);
        DebugConsole.Instance.SetBlockConsole(false);
        
        // -- Set player speed --
        if (Config.ShouldLoadIntoMain)
        {
            LocalPlayer.FpCharacter.SetWalkSpeed(LocalPlayer.FpCharacter.WalkSpeed * Config.PlayerDebugSpeed.Value);
            LocalPlayer.FpCharacter.SetRunSpeed(LocalPlayer.FpCharacter.RunSpeed * Config.PlayerDebugSpeed.Value);
        }
        
        // -- Skip Placing Animations --
        if (Config.SkipBuildingAnimations.Value && RepositioningUtils.Manager)
        {
            RepositioningUtils.Manager.SetSkipPlaceAnimations(true);
        }

        // -- Enable Bow Trajectory --
        // if (Config.EnableBowTrajectory.Value)
        // {
        //     BowTrajectory.Init();
        // }
        
        GraphicsCustomizer.Apply();
        
        LoadBootFile();
    }

    protected override void OnSonsSceneInitialized(ESonsScene sonsScene)
    {
        SUI.SUI.TogglePanel(ModManagerUi.MOD_INDICATOR_ID, sonsScene == ESonsScene.Title);
    }

    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(Config.ToggleConsoleKey.Value))
        {
            RConsole.ToggleConsole();
        }
    }

    private void LoadBootFile()
    {
        try
        {
            var bootFile = Path.Combine(LoaderEnvironment.GameRootDirectory, "boot.txt");
            if (!File.Exists(bootFile))
                return;

            var lines = File.ReadAllLines(bootFile);
            foreach (var line in lines)
            {
                DebugConsole.Instance.SendCommand(line);
            }
        }
        catch (Exception e)
        {
            RLog.Error("Error loading boot file: " + e);
        }
    }
}