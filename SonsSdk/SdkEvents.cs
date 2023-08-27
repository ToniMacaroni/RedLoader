using System.Collections;
using System.Drawing;
using System.Reflection;
using HarmonyLib;
using RedLoader;
using Sons.Cutscenes;
using Sons.Events;
using Sons.Gui.Options;
using Sons.Inventory;
using Sons.Loading;
using TheForest.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Color = UnityEngine.Color;
using Int32 = Il2CppSystem.Int32;
using Object = UnityEngine.Object;

namespace SonsSdk;

public static class SdkEvents
{
    public static readonly MelonEvent OnGameStart = new();
    public static readonly MelonEvent OnSdkInitialized = new();
    public static readonly MelonEvent OnInWorldUpdate = new();
    public static readonly MelonEvent<ItemInstance.ItemInstanceAndCount> OnItemPickup = new();
    public static readonly MelonEvent<int> OnItemCrafted = new();
    public static readonly MelonEvent<ItemInstance> OnItemConsumed = new();
    public static readonly MelonEvent OnArmorEquipped = new();

    public static readonly MelonEvent<ESonsScene> OnSonsSceneInitialized = new();

    internal static void Init()
    {
        if (_isInitialized)
        {
            return;
        }

        GlobalEvents.OnSceneWasInitialized.Subscribe(OnSceneWasInitialized, Priority.First);
        GlobalEvents.OnUpdate.Subscribe(OnUpdateInternal, Priority.First);
        
        EventRegistry.Register(GameEvent.ItemPickedUp, (EventRegistry.SubscriberCallback)SonsEventsOnItemPickedUp);
        EventRegistry.Register(GameEvent.LoadedMainScene, (EventRegistry.SubscriberCallback)SonsEventsOnGameStart);
        EventRegistry.Register(GameEvent.ItemCrafted, (EventRegistry.SubscriberCallback)SonsEventOnItemCrafted);
        EventRegistry.Register(GameEvent.ItemConsumed, (EventRegistry.SubscriberCallback)SonsEventOnItemConsumed);
        EventRegistry.Register(GameEvent.ArmorEquipped, (EventRegistry.SubscriberCallback)SonsEventOnArmorEquipped);

        _isInitialized = true;
    }

    private static void OnSceneWasInitialized(int sceneIdx, string sceneName)
    {
        switch (sceneName)
        {
            case TitleSceneName:
                MainInitializer.InitTitleScene();
                OnSonsSceneInitialized.Invoke(ESonsScene.Title);
                break;
            case LoadingSceneName:
                OnSonsSceneInitialized.Invoke(ESonsScene.Loading);
                break;
            case GameSceneName:
                OnSonsSceneInitialized.Invoke(ESonsScene.Game);
                break;
        }
    }

    private static void OnUpdateInternal()
    {
        if (!LocalPlayer.IsInWorld)
        {
            return;
        }
        
        OnInWorldUpdate.Invoke();
    }

    #region Game Events

    private static void SonsEventsOnItemPickedUp(Il2CppSystem.Object o)
    {
        var item = o.TryCast<ItemInstance.ItemInstanceAndCount>();
        if (item == null)
        {
            return;
        }
        
        OnItemPickup.Invoke(item);
    }
    
    private static void SonsEventOnItemConsumed(Il2CppSystem.Object o)
    {
        var item = o.TryCast<ItemInstance>();
        if (item == null)
        {
            return;
        }
        
        OnItemConsumed.Invoke(item);
        RLog.Msg($"Item consumed: {item.Data.Name}");
    }
    
    private static void SonsEventOnArmorEquipped(object _)
    {
        OnArmorEquipped.Invoke();
        RLog.Msg($"Armor equipped");
    }
    
    private static void SonsEventOnItemCrafted(Il2CppSystem.Object o)
    {
        var id = o.Unbox<int>();
        OnItemCrafted.Invoke(id);
    }
    
    private static void SonsEventsOnGameStart(object o)
    {
        OnGameStart.Invoke();
        ItemTools.ItemHookAdder.Flush();
    }

    #endregion

    private const string TitleSceneName = "SonsTitleScene";
    private const string LoadingSceneName = "SonsMainLoading";
    private const string GameSceneName = "SonsMain";

    private static bool _isInitialized;
    
    public enum ESonsScene
    {
        Title,
        Loading,
        Game
    }
}