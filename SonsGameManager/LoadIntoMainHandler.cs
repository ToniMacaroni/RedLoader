using System.Collections;
using System.Diagnostics;
using RedLoader;
using Sons.Loading;
using SonsSdk;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SonsGameManager;

public class LoadIntoMainHandler
{
    internal static GameObject GlobalOverlay;
    internal static TextMeshProUGUI OverlayText;

    public static IEnumerator DelayedSceneLoad()
    {
        RLog.Msg("Loading Test Game...");
        OverlayText.text = "Loading Test Game...";
        
        var wait = new WaitForSeconds(0.05f);

        var sw = new Stopwatch();
        sw.Start();

        var op = SceneManager.LoadSceneAsync("BlankScene", LoadSceneMode.Single);
        while (!op.isDone)
        {
            OverlayText.text = $"Loading Test World... (Blank:{op.progress:P0})";
            yield return wait;
        }
        
        op = SceneManager.LoadSceneAsync(SonsSceneManager.SonsMainSceneName, LoadSceneMode.Additive);
        while (!op.isDone)
        {
            OverlayText.text = $"Loading Test World... (Main:{op.progress:P0})";
            yield return wait;
        }
        
        op = SceneManager.LoadSceneAsync("SonsMainReflectionProbeBake", LoadSceneMode.Additive);
        while (!op.isDone)
        {
            OverlayText.text = $"Loading Test World... (ReflectionProbes:{op.progress:P0})";
            yield return wait;
        }
        
        sw.Stop();
        
        GlobalOverlay.SetActive(false);
        OverlayText.text = "";
        
        var ts = sw.Elapsed;
        
        SdkEvents.OnGameStart.Invoke();
        SonsTools.ShowMessage($"Test world loading took {ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}", 5);
    }

    internal static GameObject CreateBlackScreen()
    {
        var go = new GameObject("BlackScreenCanvas");
        Object.DontDestroyOnLoad(go);
        go.hideFlags |= HideFlags.HideAndDontSave;
        go.layer = 5;
        
        var canvas = go.AddComponent<Canvas>();
        
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        var blackScreen = new GameObject("BlackScreen");
        blackScreen.transform.SetParent(canvas.transform, false);
        var rect = blackScreen.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var textGo = Object.Instantiate(blackScreen, blackScreen.transform);
        var image = blackScreen.AddComponent<Image>();
        image.color = Color.black;
        OverlayText = textGo.AddComponent<TextMeshProUGUI>();
        OverlayText.fontSize = 30;
        OverlayText.color = System.Drawing.Color.PaleVioletRed.ToUnityColor();
        OverlayText.alignment = TextAlignmentOptions.Center;

        GlobalOverlay = go;
        return go;
    }
}