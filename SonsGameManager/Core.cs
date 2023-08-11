using System.Reflection;
using AdvancedTerrainGrass;
using MelonLoader;
using Sons.Input;
using SonsGameManager;
using SonsSdk;
using SonsSdk.Attributes;
using SUI;
using TheForest;
using TheForest.Utils;
using TMPro;
using UnityEngine;
using Color = System.Drawing.Color;

[assembly: SonsModInfo(typeof(Core), "SonsGameManager", "1.0.0", "Toni Macaroni")]
[assembly: MelonColor(0, 255, 20, 255)]

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

    public override void OnInitializeMelon()
    {
        Config.Load();
        GraphicsCustomizer.Load();
        GamePatches.Init();
    }

    protected override void OnSdkInitialized()
    {
        if(Config.ShouldLoadIntoMain)
            GameBootLogoPatch.DelayedSceneLoad().RunCoro();
        else
            GameBootLogoPatch.GlobalOverlay.SetActive(false);
        
    }

    protected override void OnGameStart()
    {
        Log("======= GAME STARTED ========");
        
        DebugConsole.Instance.enabled = true;
        DebugConsole.SetCheatsAllowed(true);
        DebugConsole.Instance.SetBlockConsole(false);
        
        GraphicsCustomizer.Apply();
    }

    [DebugCommand("togglegrass")]
    private void ToggleGrassCommand(string args)
    {
        if (!GrassManager._instance)
            return;

        if (args == "")
        {
            SonsSdk.SonsSdk.PrintMessage("Usage: togglegrass [on/off]");
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
            SonsSdk.SonsSdk.PrintMessage("Usage: grass [density] [distance]");
            return;
        }
        
        GraphicsCustomizer.SetGrassSettings(parts[0], parts[1]);
    }
}