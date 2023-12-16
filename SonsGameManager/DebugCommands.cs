using System.Text;
using AdvancedTerrainGrass;
using Endnight.Utilities;
using RedLoader;
using RedLoader.Utils;
using Sons.Ai.Vail.Inventory;
using Sons.Characters;
using Sons.Construction.GRABS;
using Sons.Gameplay;
using Sons.Gameplay.GameSetup;
using Sons.Items.Core;
using Sons.Lodding;
using Sons.PostProcessing;
using Sons.Prefabs;
using Sons.Save;
using SonsSdk;
using SonsSdk.Attributes;
using TheForest;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using GameState = SonsSdk.GameState;

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
    /// Clears all pickups in a radius
    /// </summary>
    /// <param name="args"></param>
    /// <command>clearpickups</command>
    [DebugCommand("clearpickups")]
    private void ClearPickupsCommand(string args)
    {
        if(!float.TryParse(args, out var radius))
            radius = 5f;
        
        var pickups = Resources.FindObjectsOfTypeAll<VailPickup>();
        foreach (var pickup in pickups)
        {
            if (Vector3.Distance(ActiveWorldLocation.Position, pickup.transform.position) > radius)
                continue;
            
            UnityEngine.Object.Destroy(pickup.gameObject);
        }
    }

    /// <summary>
    /// Go to a pickup by name (picks the first one that contains the name). Useful for finding story items.
    /// </summary>
    /// <param name="args"></param>
    /// <command>gotopickup</command>
    [DebugCommand("gotopickup")]
    private void GoToPickup(string args)
    {
        args = args.ToLower();
        
        var pickups = Resources.FindObjectsOfTypeAll<PickUp>();
        foreach (var pickup in pickups)
        {
            if (pickup.name.ToLower().Contains(args) && !pickup.transform.IsPositionNearZero())
            {
                var pos = pickup.transform.position;
                RLog.Msg(SysColor.Orange, $"Found pickup, teleporting to {pos.ToString()}...");
                LocalPlayer.CheckCaveForcedEnter(pos);
                LocalPlayer.TeleportTo(pos, LocalPlayer.Transform.rotation);
                return;
            }
        }
    }

    /// <summary>
    /// Dump various data from the game. dump [items, characters, prefabs]
    /// </summary>
    /// <param name="args"></param>
    /// <command>dump</command>
    [DebugCommand("dump")]
    private void DumpCommand(string args)
    {
        var writer = new StringBuilder();
        
        switch (args)
        {
            case "items":
                foreach (var item in ItemDatabaseManager.Items)
                {
                    writer.AppendLine($"{item.Name} ({item._id}) (Spawnable:{item._canBeSpawned})");
                }

                break;
            case "characters":
                foreach (var def in CharacterManager.Instance._definitions)
                {
                    writer.AppendLine($"{def._id}");
                    foreach (var vari in def._variations)
                    {
                        writer.AppendLine($"\tvariation:{vari._id}");
                    }
                }

                break;
            case "prefabs":
                foreach (var def in PrefabManager._instance._definitions)
                {
                    writer.AppendLine($"{def._id}");
                }

                break;
        }
        
        File.WriteAllText($"{args}.txt", writer.ToString());
    }
    
    [DebugCommand("getsaveid")]
    private void GetSaveIdCommand(string args)
    {
        RLog.Msg(SysColor.Orange, $"{GameState.LastLoadedSaveId}");
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