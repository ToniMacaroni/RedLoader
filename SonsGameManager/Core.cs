using System.Reflection;
using AdvancedTerrainGrass;
using MelonLoader;
using Sons.Input;
using SonsGameManager;
using SonsSdk;
using SonsSdk.Attributes;
using SUI;
using TheForest;
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
        GameCore.Entry();
    }

    protected override void OnSdkInitialized()
    {
        if(GameCore.ShouldLoadIntoMain)
            GameBootLogoPatch.DelayedSceneLoad().RunCoro();
        else
            GameBootLogoPatch.GlobalOverlay.SetActive(false);
        
    }

    protected override void OnGameStart()
    {
        Log("======= GAME STARTED ========");
        
        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
        HudGui.Instance.EnableCursor(false);
        
        InputSystem.Cursor.Enable(false, false);
        InputSystem.Cursor.Enable(false, true);
        InputSystem.Cursor.RefreshCursorVisibility();
        //GameCore.EnableDebugConsole();
        //DebugConsole.SetCheatsAllowed(true);
        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
    }

    [DebugCommand("togglegrass")]
    private void ToggleGrassCommand(string args)
    {
        GrassManager._instance.DoRenderGrass = args == "on";
    }
}