using System.Collections;
using System.Diagnostics;
using System.Reflection;
using AdvancedTerrainGrass;
using Construction;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using RedLoader;
using RedLoader.Utils;
using Sons.Crafting;
using Sons.Gui;
using Sons.Lodding;
using Sons.PostProcessing;
using Sons.Save;
using Sons.Weapon;
using SonsSdk;
using SonsSdk.Attributes;
using SUI;
using TheForest;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.HighDefinition;
using Color = System.Drawing.Color;

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
        GraphicsCustomizer.Load();
        GamePatches.Init();
    }
    
    protected override void OnSdkInitialized()
    {
        ModManagerUi.Create();
        //SettingsRegistry.CreateSettings(this, null, typeof(Config));
        
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

        // GlobalInput.RegisterKey(KeyCode.H, () =>
        // {
        //     if (LocalPlayer.Inventory.LeftHandItem == null || LocalPlayer.Inventory.LeftHandItem._itemID != 552)
        //         return;
        //     
        //     var book = LocalPlayer.Inventory.LeftHandItem.ItemObject.GetComponent<BlueprintBookController>();
        //     var tabPrefab = book._tabs._items[0].gameObject;
        //     var tab = UnityEngine.Object.Instantiate(tabPrefab, tabPrefab.transform.parent);
        //     var interaction = tab.GetComponent<HeldBookInteraction>();
        //     book._tabs.Add(interaction);
        //     interaction.OnInteract.AddListener((UnityAction<HeldBookInteraction>)OnTabInteract);
        // });
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
}