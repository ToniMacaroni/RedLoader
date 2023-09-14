using AdvancedTerrainGrass;
using Sons.Lodding;
using Sons.PostProcessing;
using SonsSdk;
using SonsSdk.Attributes;
using TheForest;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace SonsGameManager;

public partial class Core
{
    public static bool LogSounds;
    
    private float _oldShadowDistance;

    private static bool? ParseBool(string args, string usage)
    {
        var usageMessage = $"Usage: {usage}";
        
        if (string.IsNullOrEmpty(args))
        {
            SonsTools.ShowMessage(usageMessage);
            return null;
        }
        
        return args == "on";
    }

    /// <summary>
    /// Toggles the visibility of grass
    /// </summary>
    /// <param name="args"></param>
    /// <command>togglegrass</command>
    [DebugCommand("togglegrass")]
    private void ToggleGrassCommand(string args)
    {
        if (!GrassManager._instance)
            return;

        GrassManager._instance.DoRenderGrass = string.IsNullOrEmpty(args) ? 
            !GrassManager._instance.DoRenderGrass :
            args == "on";
    }

    /// <summary>
    /// Log sounds to the console
    /// </summary>
    /// <param name="args"></param>
    [DebugCommand("logsounds")]
    private void LogSoundsCommand(string args)
    {
        LogSounds = ParseBool(args, "logsounds [on/off]") ?? LogSounds;
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
    
    /// <summary>
    /// Equip the laser pointer that let's you use the procedural building system
    /// </summary>
    /// <param name="args"></param>
    /// <command>laserpointer</command>
    [DebugCommand("laserpointer")]
    private void LaserPointerCommand(string args)
    {
        DebugConsole.Instance._equipitem("505");
    }
    
    /// <summary>
    /// Toggles the shadow rendering (Shadows, Contact Shadows, Micro Shadowing)
    /// </summary>
    /// <param name="args"></param>
    /// <command>toggleshadows</command>
    [DebugCommand("toggleshadows")]
    private void ToggleShadowsCommand(string args)
    {
        var pp = PostProcessingManager.GetVolumeProfile();
        pp.TryGet(out HDShadowSettings shadowSettings);
        pp.TryGet(out ContactShadows contactShadows);
        pp.TryGet(out MicroShadowing microShadowing);
        
        var oldShadowDistance = shadowSettings.maxShadowDistance.value;
        var shadowsEnabled = oldShadowDistance > 1;

        if (shadowsEnabled)
        {
            _oldShadowDistance = oldShadowDistance;
        }

        shadowSettings.maxShadowDistance.Override(shadowsEnabled ? 0 : _oldShadowDistance);
        contactShadows.enable.Override(!shadowsEnabled);
        microShadowing.enable.Override(!shadowsEnabled);
    }
}