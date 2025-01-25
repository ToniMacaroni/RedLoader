using System.Diagnostics;
using System.Reflection;
using System.Text;
using AdvancedTerrainGrass;
using Endnight.Utilities;
using Il2CppInterop.Runtime;
using MonoMod.Utils;
using RedLoader;
using RedLoader.Utils;
using Sons.Ai.Vail;
using Sons.Ai.Vail.Inventory;
using Sons.Characters;
using Sons.Crafting.Structures;
using Sons.Cutscenes;
using Sons.Gameplay;
using Sons.Items.Core;
using Sons.Lodding;
using Sons.Prefabs;
using SonsGameManager;
using SonsSdk.Attributes;
using SonsSdk.Private;
using TheForest;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using Color = System.Drawing.Color;

namespace SonsSdk;

public static class GameCommands
{
    /// <summary>
    /// Toggles the visibility of grass
    /// </summary>
    /// <param name="args"></param>
    /// <command>togglegrass</command>
    [DebugCommand("togglegrass")]
    private static void ToggleGrassCommand(string args)
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
    // [DebugCommand("logsounds")]
    // private static void LogSoundsCommand(string args)
    // {
    //     LogSounds = ParseBool(args, "logsounds [on/off]") ?? LogSounds;
    // }

    /// <summary>
    /// Freecam mode without "exiting" the player
    /// </summary>
    /// <param name="args"></param>
    /// <command>xfreecam</command>
    [DebugCommand("xfreecam")]
    private static void FreecamCommand(string args)
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
    private static void CancelBlueprintsCommand(string args)
    {
        var pos = ActiveWorldLocation.Position;
        ParseFloat(args, 100, out var radius);

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
        RLog.Msg(System.Drawing.Color.Orange, "Removed " + bList.Count + " blueprints");
    }
    
    /// <summary>
    /// Finish all blueprints in a radius
    /// </summary>
    /// <param name="args"></param>
    /// <command>finishblueprints</command>
    [DebugCommand("finishblueprints")]
    private static void FinishBlueprintsCommand(string args)
    {
        var pos = ActiveWorldLocation.Position;
        ParseFloat(args, 100, out var radius);

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
        RLog.Msg(System.Drawing.Color.Orange, "Finished " + bList.Count + " blueprints");
    }

    /// <summary>
    /// Removes trees, bushes and (including billboards) for debugging purposes
    /// </summary>
    /// <param name="args"></param>
    /// <command>noforest</command>
    [DebugCommand("noforest")]
    private static void NoForestCommand(string args)
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
    private static void ClearPickupsCommand(string args)
    {
        ParseFloat(args, 5, out var radius);

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
    private static void GoToPickup(string args)
    {
        args = args.ToLower();
        
        var pickups = Resources.FindObjectsOfTypeAll<PickUp>();
        foreach (var pickup in pickups)
        {
            if (pickup.name.ToLower().Contains(args) && !pickup.transform.IsPositionNearZero())
            {
                var pos = pickup.transform.position;
                RLog.Msg(System.Drawing.Color.Orange, $"Found pickup, teleporting to {pos.ToString()}...");
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
    private static void GhostPlayerCommand(string args)
    {
        VailActorManager.SetGhostPlayer(args == "on");
    }
    
    /// <summary>
    /// Save the console position to the config.
    /// </summary>
    /// <param name="args"></param>
    /// <command>saveconsolepos</command>
    [DebugCommand("saveconsolepos")]
    private static void SaveConsolePos(string args)
    {
        CorePreferences.SaveConsoleRect();
        RLog.Msg("Saved console rect");
    }
    
    /// <summary>
    /// Add sentiment to virginia
    /// </summary>
    /// <param name="args"></param>
    /// <command>virginiasentiment</command>
    [DebugCommand("virginiasentiment")]
    private static void VirginiaSentiment(string args)
    {
        if (!ParseFloat(args, 0, out var num))
            return;

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
    private static void VirginiaVisit(string args)
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
    private static void DumpCommand(string args)
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
    private static void PlayCutsceneCommand(string args)
    {
        if (!CutsceneManager.StartCutscene(args))
        {
            Report("Couldn't play cutscene");
        }
    }
    
    [DebugCommand("getsaveid")]
    private static void GetSaveIdCommand(string args)
    {
        Report(GameState.LastLoadedSaveId.ToString());
    }

    /// <summary>
    /// Toggles the shadow rendering (Shadows, Contact Shadows, Micro Shadowing)
    /// </summary>
    /// <param name="args"></param>
    /// <command>toggleshadows</command>
    // [DebugCommand("toggleshadows")]
    // private static void ToggleShadowsCommand(string args)
    // {
    //     var pp = PostProcessingManager.GetVolumeProfile();
    //     pp.TryGet(out HDShadowSettings shadowSettings);
    //     pp.TryGet(out ContactShadows contactShadows);
    //     pp.TryGet(out MicroShadowing microShadowing);
    //     
    //     var oldShadowDistance = shadowSettings.maxShadowDistance.value;
    //     var shadowsEnabled = oldShadowDistance > 1;
    //
    //     if (shadowsEnabled)
    //     {
    //         _oldShadowDistance = oldShadowDistance;
    //     }
    //
    //     shadowSettings.maxShadowDistance.Override(shadowsEnabled ? 0 : _oldShadowDistance);
    //     contactShadows.enable.Override(!shadowsEnabled);
    //     microShadowing.enable.Override(!shadowsEnabled);
    // }

    [DebugCommand("dumpboltserializers")]
    private static void DumpBoltSerializersCommand(string args)
    {
        var str = new StringBuilder();
        foreach (var (id, factory) in Bolt.Factory._factoriesByKey)
        {
            var type = Il2CppType.TypeFromPointer(factory.ObjectClass);
            str.AppendLine($"{{ESerializer.{type.Name}, \"{id.IdString}\"}},");
        }

        str.AppendLine();
        
        foreach (var (id, factory) in Bolt.Factory._factoriesByKey)
        {
            var type = Il2CppType.TypeFromPointer(factory.ObjectClass);

            str.AppendLine($"{type.Name},");
        }
        
        RLog.MsgDirect(str.ToString());
        File.WriteAllText("BoltSerializers.txt", str.ToString());

        RLog.MsgDirect(System.Drawing.Color.Orange, "Seriaizers dumped to BoltSerializers.txt");
    }
    
    [DebugCommand("placestructure")]
    private static void PlaceStructure(string args)
    {
        ConstructionTools.PlaceStructureInteractive(int.Parse(args));
    }

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
    
    private static bool ParseFloat(string args, float defaultValue, out float value)
    {
        if (string.IsNullOrEmpty(args))
        {
            value = defaultValue;
            return false;
        }

        return float.TryParse(args, out value);
    }

    private static void Report(string text, bool warning = true)
    {
        SonsTools.ShowMessage(text);
        
        if(warning)
            RLog.Warning(text);
        else
            RLog.Msg(text);
    }

    internal static void Init()
    {
        RegisterFromType(typeof(GameCommands));
    }
    
    public static void RegisterFromType(Type targetType, object instance = null)
    {
        var methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (MethodInfo method in methods)
        {
            var attribute = method.GetCustomAttribute<DebugCommandAttribute>();
            if (attribute != null)
            {
                var fastDelegate = method.GetFastDelegate();

                bool Wrapper(string s)
                {
                    var ret = fastDelegate.Invoke(instance, s);
                    if (ret is bool b)
                        return b;
                    return true;
                }
                
                DebugConsole.RegisterCommand(attribute.Command, (Il2CppSystem.Func<string, bool>)Wrapper, DebugConsole.Instance);
                
                RLog.Msg($"Registered command '{attribute.Command}'");
            }
        }
    }
}
