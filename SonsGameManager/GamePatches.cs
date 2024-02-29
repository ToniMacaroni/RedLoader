using System.Collections;
using System.Reflection;
using Bolt;
using Endnight.Editor;
using Harmony;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using RedLoader;
using Sons.Cutscenes;
using Sons.Gui;
using Sons.Inventory;
using Sons.Multiplayer.Dedicated;
using Sons.Music;
using Sons.TerrainDetail;
using SonsSdk;
using TheForest.Player.Actions;
using TheForest.Utils;
using UnityEngine;
using Color = System.Drawing.Color;
using Logger = HarmonyLib.Tools.Logger;
using Object = Il2CppSystem.Object;

// ReSharper disable InconsistentNaming

namespace SonsGameManager;

public class GamePatches
{
    private static ConfiguredPatcher<GamePatches> _patcher;
    
    public static void Init()
    {
        _patcher = new(Core.HarmonyInst);
        
        _patcher.Prefix<SonsLaunch>(nameof(SonsLaunch.Start), nameof(LaunchStartPatch));
        _patcher.Prefix<SonsFMODEventEmitter>(nameof(SonsFMODEventEmitter.Play), nameof(SonsEmitterPlayPatch));
        _patcher.Prefix<FMOD_StudioEventEmitter>(nameof(FMOD_StudioEventEmitter.StartEvent), nameof(FModEmitterPlayPatch));
        
        var mt = typeof(MusicManager).GetMethod(nameof(MusicManager.SetMusicEvent), BindingFlags.Public | BindingFlags.Instance);
        var hm = new HarmonyMethod(typeof(GamePatches).GetMethod(nameof(MusicLookupPatch), BindingFlags.NonPublic | BindingFlags.Static));
        Core.HarmonyInst.Patch(mt, hm);

        if (Config.RedirectDebugLogs.Value)
        {
            _patcher.Prefix<Debug>(nameof(Debug.Log), nameof(LogPatch), true, typeof(Object));
            _patcher.Prefix<Debug>(nameof(Debug.LogWarning), nameof(LogWarningPatch), true, typeof(Object));
            _patcher.Prefix<Debug>(nameof(Debug.LogError), nameof(LogErrorPatch), true, typeof(Object));
        }

        if (Config.ShouldLoadIntoMain)
        {
            if(Config.DontAutoAddScenes.Value)
                AutoAddScene.DisableAll();
            
            if (Config.DontLoadSaves.Value)
            {
                _patcher.Prefix<LoadSave>(nameof(LoadSave.Awake), nameof(LoadSavePatch));
                _patcher.Prefix<LoadSave>(nameof(LoadSave.Start), nameof(LoadSavePatch));    
            }
            
            if(!Config.ActivateWorldObjects.Value)
                _patcher.Prefix<WorldObjectLocatorManager>(nameof(WorldObjectLocatorManager.OnEnable), nameof(WorldActivatorPatch));
        }
        
        _patcher.Prefix<PauseMenu>(nameof(PauseMenu.OnEnable), nameof(PauseMenuPatch));
        
        _patcher.Prefix<PlayerConsumeItemAction>(nameof(PlayerConsumeItemAction.ConsumeItem), nameof(ConsumePrefix));
        _patcher.Prefix<InventoryCutscene>(nameof(InventoryCutscene.Play), nameof(InventoryCutscenePlay));
        
        _patcher.Patch(typeof(PickupPatch));
    }

    private static void LaunchStartPatch(SonsLaunch __instance)
    {
        RLog.Msg("===== Launch Start! =====");

        LoadIntoMainHandler.CreateBlackScreen();
        if (Config.SkipIntro.Value)
        {
            __instance._titleSceneLoader._delay = 0f;
        }
        else
        {
            LoadIntoMainHandler.GlobalOverlay.SetActive(false);
        }
    }
    
    private static bool SonsEmitterPlayPatch(SonsFMODEventEmitter __instance)
    {
        var eventPath = __instance._eventPath;
        if(Core.LogSounds)
            RLog.Msg(Color.Green, "SonsEmitter sound: " + eventPath);
        if (SoundTools.EventRedirects.TryGetValue(eventPath, out var newPath))
        {
            __instance.SetEventPath(newPath);
            RLog.Debug($"\tRedirected to {newPath}");
        }
        return !Config.SavedMutesSounds.Contains(eventPath);
    }
    
    private static bool FModEmitterPlayPatch(FMOD_StudioEventEmitter __instance)
    {
        var eventPath = __instance._eventPath;
        if(Core.LogSounds)
            RLog.Msg(Color.Green, "FModEmitter sound: " + eventPath);
        if (SoundTools.EventRedirects.TryGetValue(eventPath, out var newPath))
        {
            __instance.SetEventPath(newPath);
            RLog.Debug($"\tRedirected to {newPath}");
        }
        __instance._forcedDisabled = Config.SavedMutesSounds.Contains(eventPath) || __instance._forcedDisabled;
        return true;
    }
    
    private static void MusicLookupPatch(MusicManager __instance, ref string eventPath)
    {
        RLog.Debug($"MusicManager.SetMusicEvent({eventPath})");
        if (SoundTools.EventRedirects.TryGetValue(eventPath, out var newPath))
        {
            eventPath = newPath;
            RLog.Debug($"\tRedirected to {newPath}");
        }
    }

    private static void ConsumePrefix(ItemInstance itemInstance, ref bool shouldPlayConsumeAnimation)
    {
        if (Config.NoConsumeAnimation.Value && (itemInstance == null || itemInstance._data.Id != 517))
            shouldPlayConsumeAnimation = false;
    }
    
    private static void InventoryCutscenePlay(InventoryCutscene __instance)
    {
        if (!Config.InstantInventoryOpen.Value)
            return;
        
        __instance._inventoryBagAnimator.speed = 200;
        if(__instance._layoutGroupsRolloutAnimator)
            __instance._layoutGroupsRolloutAnimator.speed = 200;
        LocalPlayer.Animator.speed = 200;
    }

    [HarmonyPatch(typeof(Sons.Gameplay.PickUp), nameof(Sons.Gameplay.PickUp.MainEffect))]
    public static class PickupPatch
    {
        public static void Prefix(Sons.Gameplay.PickUp __instance, out bool __state)
        {
            __state = __instance._preventAutoEquip;

            if (Config.NoAutoEquipStones.Value && __instance._itemId is 476 or 393)
            {
                var inv = LocalPlayer.Inventory;
                var isFull = inv.AmountOf(__instance._itemId) >= inv.GetMaxAmountOf(__instance._itemId);
                
                __instance._preventAutoEquip = !isFull;
            }
        }
    
        public static void Postfix(Sons.Gameplay.PickUp __instance, bool __state)
        {
            __instance._preventAutoEquip = __state;
        }
    }
    
    private static void LogPatch(Object message) => RLog.MsgDirect(Color.DarkGray, $"[Unity] {message.ToString()}");
    private static void LogWarningPatch(Object message) => RLog.MsgDirect(Color.Yellow, $"[Unity] {message.ToString()}");
    private static void LogErrorPatch(Object message) => RLog.MsgDirect(Color.IndianRed, $"[Unity] {message.ToString()}");
    
    private static void PauseMenuPatch(PauseMenu __instance)
    {
        RLog.DebugBig("PauseMenu created!");
        SUI.SUI.OnPauseMenuCreated(__instance);
    }

    private static bool LoadSavePatch()
    {
        RLog.Debug("Stopped LoadSave");
        return false;
    }

    private static bool WorldActivatorPatch()
    {
        RLog.Debug("Stopped WorldObject activation");
        return false;
    }
}

// [HarmonyPatch(typeof(BoltLauncher), nameof(BoltLauncher.GetGlobalBehaviourTypes))]
// public static class GetGlobalBehaviourTypesPatch
// {
//     public static void Postfix(ref Il2CppSystem.Collections.Generic.List<STuple<BoltGlobalBehaviourAttribute, Il2CppSystem.Type>> __result)
//     {
//         RLog.Msg(System.Drawing.Color.Orange, $"BoltLauncher.GetGlobalBehaviourTypes()");
//         var attr = new BoltGlobalBehaviourAttribute();
//         var type = Il2CppType.Of<ModdedGlobalEvents>();
//         __result.Add(new STuple<BoltGlobalBehaviourAttribute, Il2CppSystem.Type>(attr, type));
//         RLog.Msg(System.Drawing.Color.Orange, $"Added global event");
//     }
// }