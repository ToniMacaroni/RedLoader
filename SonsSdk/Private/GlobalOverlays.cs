using RedLoader;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SonsSdk.Private;

internal class GlobalOverlays
{
    public static GameObject OverlayContainer;
    public static Image BlackScreenImage;
    public static TextMeshProUGUI OverlayText;
    public static ProgressBarContainer ProgressBar;
    
    public static GameObject CreateBlackScreen()
    {
        var go = new GameObject("BlackScreenCanvas").DontDestroyOnLoad().HideAndDontSave();
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
        var textGo = blackScreen.Instantiate().SetParent(blackScreen.transform);
        BlackScreenImage = blackScreen.AddComponent<Image>();
        BlackScreenImage.color = Color.black;
        OverlayText = textGo.AddComponent<TextMeshProUGUI>();
        OverlayText.fontSize = 30;
        OverlayText.color = System.Drawing.Color.PaleVioletRed.ToUnityColor();
        OverlayText.alignment = TextAlignmentOptions.Center;

        ProgressBar = CreateProgressbar(go.GetComponent<RectTransform>());

        OverlayContainer = go;
        return go;
    }

    private static ProgressBarContainer CreateProgressbar(RectTransform container)
    {
        var bar = container.gameObject.AddGo("ProgressBarBackground");
        var barRect = bar.AddComponent<RectTransform>();
        barRect.anchorMin = new(0.1f, 0f);
        barRect.anchorMax = new(0.9f, 0f);
        barRect.sizeDelta = new(0, 30);
        barRect.pivot = new(0.5f, 0);
        barRect.anchoredPosition = new(0, 200);
        var barImage = bar.AddComponent<Image>();
        barImage.color = Color.grey;

        var fore = bar.AddGo("ProgressBarForeground");
        var foreRect = fore.AddComponent<RectTransform>();
        foreRect.anchorMin = Vector2.zero;
        foreRect.anchorMax = new(0, 1);
        foreRect.sizeDelta = Vector2.zero;
        foreRect.pivot = new(0, 0.5f);
        var foreImage = fore.AddComponent<Image>();
        foreImage.color = new(0.735849f, 0.1087575f, 0.3132702f);

        return new ProgressBarContainer
        {
            ProgressbarBackground = barRect,
            ProgressbarBackgroundImage = barImage,
            ProgressbarForeground = foreRect,
            ProgressbarForegroundImage = foreImage
        };
    }

    public static void ToggleOverlay(bool enable)
    {
        if (!OverlayContainer)
        {
            return;
        }
        
        OverlayContainer.SetActive(enable);
    }

    public static void SetProgressbarOffset(float y)
    {
        if (ProgressBar == null)
        {
            return;
        }

        ProgressBar.ProgressbarBackground.anchoredPosition = new(0, y);
    }

    public static void ToggleBackground(bool enable)
    {
        if (!BlackScreenImage)
        {
            return;
        }

        BlackScreenImage.enabled = enable;
    }

    internal class ProgressBarContainer
    {
        public RectTransform ProgressbarBackground;
        public Image ProgressbarBackgroundImage;
        public RectTransform ProgressbarForeground;
        public Image ProgressbarForegroundImage;

        public float CurrentProgress { get; private set; }

        public void SetProgress(float progress)
        {
            ProgressbarForeground.anchorMax = new(progress, 1);
        }

        public void AddProgress(float progress)
        {
            CurrentProgress = Mathf.Clamp01(CurrentProgress + progress);
            SetProgress(CurrentProgress);
        }
    }
}
