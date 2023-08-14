using System.Reflection;
using AdvancedTerrainGrass;
using Construction;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using Sons.Input;
using Sons.Items.Core;
using Sons.Weapon;
using SonsGameManager;
using SonsSdk;
using SonsSdk.Attributes;
using SUI;
using TheForest;
using TheForest.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Color = System.Drawing.Color;

namespace SonsGameManager;

using static SUI.SUI;

public class Core : SonsMod
{
    internal static Core Instance;

    internal static HarmonyLib.Harmony HarmonyInst => Instance.HarmonyInstance;
    internal static MelonLogger.Instance Logger => Instance.LoggerInstance;

    public Core()
    {
        Instance = this;
    }

    internal static void Log(string text)
    {
        Logger.Msg(Color.PaleVioletRed, text);
    }

    public override void OnInitializeMod()
    {
        Config.Load();
        GraphicsCustomizer.Load();
        GamePatches.Init();
    }

    protected override void OnSdkInitialized()
    {
        if(Config.ShouldLoadIntoMain)
        {
            GameBootLogoPatch.DelayedSceneLoad().RunCoro();
            return;
        }
        
        GameBootLogoPatch.GlobalOverlay.SetActive(false);

        var mainBgBlack = new UnityEngine.Color(0, 0, 0, 0.8f);
        var componentBlack = new UnityEngine.Color(0, 0, 0, 0.6f);
        
        ModManagerUi.CreateUi();

        // _ = RegisterNewPanel("ModListPanel")
        //     .Dock(EDockType.Fill).RectPadding(500).Background(mainBgBlack, EBackground.Rounded);
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
    }

    protected override void OnSonsSceneInitialized(SdkEvents.ESonsScene sonsScene)
    {
        TogglePanel(ModManagerUi.MOD_INDICATOR_ID, sonsScene == SdkEvents.ESonsScene.Title);
    }

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
}