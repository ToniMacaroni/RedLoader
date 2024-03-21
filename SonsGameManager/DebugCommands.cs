using System.Text;
using System.Text.RegularExpressions;
using AdvancedTerrainGrass;
using Endnight.Environment;
using Endnight.Types;
using Endnight.Utilities;
using RedLoader;
using RedLoader.Utils;
using Sons.Ai.Vail;
using Sons.Ai.Vail.Inventory;
using Sons.Areas;
using Sons.Characters;
using Sons.Crafting.Structures;
using Sons.Cutscenes;
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
using Random = UnityEngine.Random;

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

    private static void Report(string text, bool warning = true)
    {
        SonsTools.ShowMessage(text);
        
        if(warning)
            RLog.Warning(text);
        else
            RLog.Msg(text);
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
    /// Cancel all blueprints in a radius
    /// </summary>
    /// <param name="args"></param>
    /// <command>cancelblueprints</command>
    [DebugCommand("cancelblueprints")]
    private void CancelBlueprintsCommand(string args)
    {
        var pos = ActiveWorldLocation.Position;
        var radius = 100f;
        if (!string.IsNullOrEmpty(args))
        {
            radius = float.Parse(args);
        }

        var bList = new List<StructureCraftingNode>();

        foreach (var craftingNode in StructureCraftingSystem._instance._activeStructureNodes)
        {
            if (Vector3.Distance(craftingNode.transform.position, pos) < radius)
            {
                bList.Add(craftingNode);
            }
        }
        
        foreach (var craftingNode in bList)
        {
            craftingNode.CancelStructure();
        }
        
        SonsTools.ShowMessage("Removed " + bList.Count + " blueprints");
        RLog.Msg(SysColor.Orange, "Removed " + bList.Count + " blueprints");
    }
    
    /// <summary>
    /// Finish all blueprints in a radius
    /// </summary>
    /// <param name="args"></param>
    /// <command>finishblueprints</command>
    [DebugCommand("finishblueprints")]
    private void FinishBlueprintsCommand(string args)
    {
        var pos = ActiveWorldLocation.Position;
        var radius = 100f;
        if (!string.IsNullOrEmpty(args))
        {
            radius = float.Parse(args);
        }

        var bList = new List<StructureCraftingNode>();

        foreach (var craftingNode in StructureCraftingSystem._instance._activeStructureNodes)
        {
            if (Vector3.Distance(craftingNode.transform.position, pos) < radius)
            {
                bList.Add(craftingNode);
            }
        }
        
        foreach (var craftingNode in bList)
        {
            craftingNode.StructureCompleted();
        }
        
        SonsTools.ShowMessage("Finished " + bList.Count + " blueprints");
        RLog.Msg(SysColor.Orange, "Finished " + bList.Count + " blueprints");
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
    /// Will make the ai ignore the player.
    /// </summary>
    /// <param name="args"></param>
    /// <command>aighostplayer</command>
    [DebugCommand("aighostplayer")]
    public void GhostPlayerCommand(string args)
    {
        VailActorManager.SetGhostPlayer(args == "on");
    }
    
    /// <summary>
    /// Save the console position to the config.
    /// </summary>
    /// <param name="args"></param>
    /// <command>saveconsolepos</command>
    [DebugCommand("saveconsolepos")]
    public void SaveConsolePos(string args)
    {
        RConsole.SaveConsoleRect();
    }
    
    /// <summary>
    /// Add sentiment to virginia
    /// </summary>
    /// <param name="args"></param>
    /// <command>virginiasentiment</command>
    [DebugCommand("virginiasentiment")]
    private void VirginiaSentiment(string args)
    {
        float num;
        if (!float.TryParse(args, out num))
        {
            return;
        }
        
        VailWorldSimulation vailWorldSimulation;
        if (VailWorldSimulation.TryGetInstance(out vailWorldSimulation))
        {
            vailWorldSimulation.AddVirginiaSentiment(LocalPlayer.PlayerStimuli, num);
        }
    }
    
    /// <summary>
    /// Invokes a virginia visit event
    /// </summary>
    /// <param name="args"></param>
    /// <command>virginiavisit</command>
    [DebugCommand("virginiavisit")]
    private void VirginiaVisit(string args)
    {
        VailWorldEvents vailWorldEvents;
        if (VailWorldEvents.TryGetInstance(out vailWorldEvents))
        {
            vailWorldEvents.DebugVirginiaVisit(0);
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
                 foreach (var def in GameManagers.GetCharacterManager()._definitions)
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
    
    /// <summary>
    /// Play a cutscene by name
    /// </summary>
    /// <param name="args"></param>
    /// <command>playcutscene</command>
    [DebugCommand("playcutscene")]
    private void PlayCutsceneCommand(string args)
    {
        if (!CutsceneManager.StartCutscene(args))
        {
            Report("Couldn't play cutscene");
        }
    }
    
    [DebugCommand("getsaveid")]
    private void GetSaveIdCommand(string args)
    {
        Report(GameState.LastLoadedSaveId.ToString());
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