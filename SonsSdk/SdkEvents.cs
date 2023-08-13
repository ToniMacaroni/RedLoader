using System.Collections;
using System.Drawing;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using Sons.Cutscenes;
using Sons.Events;
using Sons.Gui.Options;
using Sons.Loading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Color = UnityEngine.Color;
using Object = UnityEngine.Object;

namespace SonsSdk;

public static class SdkEvents
{
    public static readonly MelonEvent OnGameStart = new();
    public static readonly MelonEvent OnSdkInitialized = new();
    
    public static readonly MelonEvent<ESonsScene> OnSonsSceneInitialized = new();

    internal static void Init()
    {
        if (_isInitialized)
        {
            return;
        }
        
        Patches.InitPatches();
        
        MelonEvents.OnSceneWasInitialized.Subscribe(OnSceneWasInitialized, Priority.First);

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