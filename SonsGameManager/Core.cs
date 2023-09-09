using System.Diagnostics;
using System.Reflection;
using AdvancedTerrainGrass;
using Construction;
using Il2CppInterop.Runtime.Injection;
using RedLoader;
using RedLoader.Utils;
using Sons.Crafting;
using Sons.Gui;
using Sons.Lodding;
using Sons.Save;
using Sons.Weapon;
using SonsSdk;
using SonsSdk.Attributes;
using TheForest;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Events;
using Color = System.Drawing.Color;

namespace SonsGameManager;

public class Core : SonsMod
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
        GraphicsCustomizer.Load();
        GamePatches.Init();
    }
    
    protected override void OnSdkInitialized()
    {
        if(Config.ShouldLoadIntoMain)
        {
            LoadIntoMainHandler.DelayedSceneLoad().RunCoro();
            return;
        }

        LoadIntoMainHandler.GlobalOverlay.SetActive(false);

        ModManagerUi.Create();

        LoadTests();

        if (!string.IsNullOrEmpty(Config.LoadSaveGame) && int.TryParse(Config.LoadSaveGame, out var id))
        {
            Resources.FindObjectsOfTypeAll<LoadMenu>()
                .First(x=>x.name=="SinglePlayerLoadPanel")
                .LoadSlotActivated(id, SaveGameManager.GetData<GameStateData>(SaveGameType.SinglePlayer, id));
        }
        
        SUI.SUI.CreateSettings(this, null, typeof(Config));
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
        if (Config.EnableBowTrajectory.Value)
        {
            BowTrajectory.Init();
        }
        
        GraphicsCustomizer.Apply();
        
        LoadBootFile();

        GlobalInput.RegisterKey(KeyCode.H, () =>
        {
            var book = LocalPlayer.Inventory.LeftHandItem.ItemObject.GetComponent<BlueprintBookController>();
            var tabPrefab = book._tabs._items[0].gameObject;
            var tab = UnityEngine.Object.Instantiate(tabPrefab, tabPrefab.transform.parent);
            tab.transform.localPosition += new Vector3(0 - 0.2f, 0);
            var interaction = tab.GetComponent<HeldBookInteraction>();
            book._tabs.Add(interaction);
            interaction.OnInteract.AddListener((UnityAction<HeldBookInteraction>)OnTabInteract);
        });
    }

    private void OnTabInteract(HeldBookInteraction interaction)
    {
        
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

    #region DebugCommands

    /// <summary>
    /// Toggles the visibility of grass
    /// </summary>
    /// <param name="args"></param>
    /// <command>togglegrass [on/off]</command>
    [DebugCommand("togglegrass")]
    private void ToggleGrassCommand(string args)
    {
        if (!GrassManager._instance)
            return;

        if (string.IsNullOrEmpty(args))
        {
            SonsTools.ShowMessage("Usage: togglegrass [on/off]");
            return;
        }
        
        GrassManager._instance.DoRenderGrass = args == "on";
    }
    
    /// <summary>
    /// Adjusts the grass density and visibility distance
    /// </summary>
    /// <param name="args"></param>
    /// <command>grass [density] [distance]</command>
    /// <example>grass 0.5 120</example>
    [DebugCommand("grass")]
    private void GrassCommand(string args)
    {
        var parts = args.Split(' ').Select(float.Parse).ToArray();
        
        if (parts.Length != 2)
        {
            SonsTools.ShowMessage("Usage: grass [density] [distance]");
            return;
        }
        
        GraphicsCustomizer.SetGrassSettings(parts[0], parts[1]);
    }

    /// <summary>
    /// Freecam mode without "exiting" the player
    /// </summary>
    /// <param name="args"></param>
    /// <command>xfreecam</command>
    [DebugCommand("xfreecam")]
    private void FreecamCommand(string args)
    {
        var freecam = LocalPlayer.Transform.GetComponent<CustomFreeCam>();
        if (freecam)
        {
            UnityEngine.Object.Destroy(freecam);
            return;
        }
        
        LocalPlayer.Transform.gameObject.AddComponent<CustomFreeCam>();
    }
    
    /// <summary>
    /// Removes trees, bushes and (including billboards) for debugging purposes
    /// </summary>
    /// <param name="args"></param>
    /// <command>noforest</command>
    [DebugCommand("noforest")]
    private void NoForestCommand(string args)
    {
        var isActive = PathologicalGames.PoolManager.Pools["Trees"].gameObject.activeSelf;
        
        foreach (LodSettingsTypeEnum value in Enum.GetValues(typeof(LodSettingsTypeEnum)))
        {
            CustomBillboardManager.SetBillboardMask(value, isActive);
        }
        
        PathologicalGames.PoolManager.Pools["Trees"].gameObject.SetActive(!isActive);
        PathologicalGames.PoolManager.Pools["Bushes"].gameObject.SetActive(!isActive);
        PathologicalGames.PoolManager.Pools["SmallTree"].gameObject.SetActive(!isActive);
    }

    #endregion
}