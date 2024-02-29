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
    /// Go to a coordinate, goto-location or gameobject.
    /// </summary>
    /// <param name="args"></param>
    /// <command>goto</command>
    [DebugCommand("goto")]
    private void GoTo(string args)
    {
        if (string.IsNullOrEmpty(args))
        {
            RLog.Msg("$> usage: goto <GameObjectName>");
            return;
        }
        LocalPlayer.CamRotator._originalRotation = Quaternion.identity;
        LocalPlayer.MainRotator._originalRotation = Quaternion.identity;
        GameObject gameObject = FindObjectAdvanced(args + "Goto");
        if (gameObject == null)
        {
            RLog.Msg("No gotos found, searching for objects");
            gameObject = FindObjectAdvanced(args);
        }
        if (gameObject == null)
        {
            RLog.Msg("No active objects found, searching for inactive objects");
            gameObject = FindObjectAdvanced(args, StringComparison.InvariantCultureIgnoreCase, true);
        }
        if ((bool)gameObject)
        {
            GotoTarget(gameObject);
            return;
        }
        string[] array = args.Split(' ');
        if (array.Length >= 3)
        {
            if (array.All((string c) => float.TryParse(c, out var _)))
            {
                Vector3 targetPos = new Vector3(float.Parse(array[0]), Mathf.Ceil(float.Parse(array[1])), float.Parse(array[2]));
                GotoPosition(targetPos);
                return;
            }
        }
        RLog.Msg("$> '" + args + "' not found, cancelling goto");
    }
    
    private static GameObject FindObjectAdvanced(string arg, StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase, bool inactive = false)
    {
        List<string> list = arg.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
        if (list.Count == 0)
        {
            return null;
        }
        string text = list.Last();
        if (!int.TryParse(text, out var result))
        {
            result = 1;
        }
        else
        {
            int num = arg.LastIndexOf(text);
            arg = arg.Substring(0, num - 1);
        }
        result--;
        if (FindAllObjects(arg, out var allGameObjects, comparisonType, inactive))
        {
            return null;
        }
        if (result >= allGameObjects.Count)
        {
            return allGameObjects.Last();
        }
        return allGameObjects[result];
    }
    
    private static bool FindAllObjects(string arg, out List<GameObject> allGameObjects, StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase, bool inactive = false)
    {
        allGameObjects = (from eachGo in UnityEngine.Object.FindObjectsOfType<GameObject>(inactive)
            where MatchGameObjectName(arg, eachGo, comparisonType)
            orderby eachGo.GetInstanceID()
            select eachGo).ToList();
        if (allGameObjects == null || allGameObjects.Count == 0)
        {
            return true;
        }
        return false;
    }
    
    private static bool MatchGameObjectName(string arg, GameObject eachGo, StringComparison comparisonType)
    {
        string input = eachGo.name;
        if (string.Equals(eachGo.name, arg, comparisonType))
        {
            return true;
        }
        RegexOptions regexOptions = RegexOptions.None;
        switch (comparisonType)
        {
            case StringComparison.CurrentCultureIgnoreCase:
            case StringComparison.OrdinalIgnoreCase:
                regexOptions |= RegexOptions.IgnoreCase;
                break;
            case StringComparison.InvariantCulture:
                regexOptions |= RegexOptions.CultureInvariant;
                break;
            case StringComparison.InvariantCultureIgnoreCase:
                regexOptions |= RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
                break;
        }
        return Regex.IsMatch(input, "^" + arg + "$", regexOptions);
    }
    
    private void GotoTarget(GameObject target)
    {
        int layerMask = (1 << LayerMask.NameToLayer("Terrain")) | (1 << LayerMask.NameToLayer("BasicCollider"));
        if (!CheckForGotoPos(target, layerMask, out var info))
        {
            Debug.Log("$> didn't find a suitable landing spot raycasting down on '" + target.name + "', cancelling goto");
            return;
        }
        LocalPlayer.CheckCaveForcedEnter(target.transform.position);
        LocalPlayer.TeleportTo(info.point, target.transform.rotation);
        RLog.Msg("$> going to " + target.name);
    }
    
    private static bool CheckForGotoPos(GameObject target, int layerMask, out RaycastHit info)
    {
        Vector3 position = target.transform.position;
        if (Physics.SphereCast(position + Vector3.up, 0.1f, Vector3.down, out info, 60f, layerMask))
        {
            return true;
        }
        if (Physics.SphereCast(TerrainUtilities.GetTerrainPosition(position) + Vector3.up * 5f, 0.1f, Vector3.down, out info, 60f, layerMask))
        {
            return true;
        }
        return false;
    }
    
    private void GotoPosition(Vector3 targetPos)
    {
        LocalPlayer.CheckCaveForcedEnter(targetPos);
        LocalPlayer.TeleportTo(targetPos, LocalPlayer.Transform.rotation);
        Vector3 vector = targetPos;
        RLog.Msg("$> going to " + vector);
    }
    
    /// <summary>
    /// Spawn a new character into the world.
    /// </summary>
    /// <param name="args"></param>
    /// <command>addcharacter</command>
    [DebugCommand("addcharacter")]
    private void SpawnCharacter(string args)
    {
        args = args.ToLower();

        var type = VailTypes.FindActorType(args);
        if (type == VailActorTypeId.None)
        {
            var klass = VailTypes.FindActorClass(args);
            if (klass == VailActorClassId.None)
            {
                Report("Couldn't find character with id or class: " + args);
                return;
            }
            
            foreach (var entry in VailTypes._typeToClass._entries)
            {
                RLog.Msg($"Val: {entry.value}");
            }

            var foundTypes = ActorTools.GetActorTypesOfClass(klass);

            if (foundTypes.Count == 0)
            {
                Report("Couldn't find character with class: " + args);
                return;
            }

            type = foundTypes.GetRandomEntry();
        }
        
        var newActor = ActorTools.Spawn(type, SonsTools.GetPositionInFrontOfPlayer(2, 2));
        
        if (newActor == null)
        {
            Report("Failed to spawn character with id: " + args);
            return;
        }

        newActor._worldSimActor.IsDebugSpawned = true;
        
        RLog.Msg("Spawned character with id: " + args);
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
                 // foreach (var def in PrefabManager._instance._definitions)
                 // {
                 //     writer.AppendLine($"{def._id}");
                 // }
                 //TODO: 1.0 Update
                 RLog.Error("Not supported in 1.0");
        
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