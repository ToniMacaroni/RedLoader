using Il2CppInterop.Runtime.Injection;
using RedLoader;
using SUI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Color = UnityEngine.Color;

using static SUI.SUI;

namespace SonsSdk;

public class TooltipProvider
{
    private static bool _initialized;
    
    private static Canvas _canvas;
    private static GameObject _tooltipObject;
    private static RectTransform _tooltipRect;
    private static RectTransform _textRect;
    private static Observable<string> _tooltipText = new("");
    private static GraphicRaycaster _currentRaycaster;

    private static readonly BackgroundDefinition TooltipBackground = new("#000", GetBackgroundSprite(EBackground.Round10), Image.Type.Sliced);
    
    private static readonly BackgroundDefinition CardButtonBg = new(
        "#222", 
        GetBackgroundSprite(EBackground.ShadowPanel),
        Image.Type.Sliced);
    
    public static void Setup()
    {
        if (_initialized)
            return;

        _initialized = true;
        
        var go = new GameObject("TooltipCanvas");
        Object.DontDestroyOnLoad(go);
        go.hideFlags |= HideFlags.HideAndDontSave;
        go.layer = 5;
        
        _canvas = go.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 999;
        
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0;
        
        CreateTooltip(_canvas);
        _canvas.gameObject.SetActive(false);
    }

    private static void CreateTooltip(Canvas canvas)
    {
        SLabelOptions label;
        var tooltip = SContainer.Pivot(0, 0).Anchor(AnchorType.BottomLeft).Position(0,0).Size(400, 100).Background(CardButtonBg)
                      - (label = SLabel.Bind(_tooltipText).Dock(EDockType.Fill).FontSize(18).Alignment(TextAlignmentOptions.Center));
        _tooltipObject = tooltip.Root;
        _tooltipRect = tooltip.RectTransform;
        _textRect = label.RectTransform;
        var csf = label.Root.AddComponent<ContentSizeFitter>();
        csf.verticalFit = csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        _tooltipObject.AddComponent<FollowMouseHandler>();
        tooltip.SetParent(canvas.transform);

        HideTooltip();
    }
    
    public static void ShowTooltip(string text)
    {
        _tooltipText.Value = text;
        
        var mousePos = Input.mousePosition;
        _tooltipRect.anchoredPosition = new Vector2(mousePos.x * (1920f / Screen.width), mousePos.y * (1080f / Screen.height));
        var rectWidth = _textRect.rect.width;
        _tooltipRect.sizeDelta = new Vector2(rectWidth + 100, 100);
        
        _tooltipObject.SetActive(true);
    }
    
    public static void HideTooltip()
    {
        _tooltipObject.SetActive(false);
    }

    public static void RaycastFor(GraphicRaycaster raycaster)
    {
        _currentRaycaster = raycaster;
        _canvas.gameObject.SetActive(true);
        
        GlobalEvents.OnUpdate.Unsubscribe(OnUpdate);
        GlobalEvents.OnUpdate.Subscribe(OnUpdate);
    }

    public static void StopRaycasting()
    {
        GlobalEvents.OnUpdate.Unsubscribe(OnUpdate);
        
        _currentRaycaster = null;
        
        _canvas.gameObject.SetActive(false);
    }

    private static void OnUpdate()
    {
        if (!_currentRaycaster)
            return;

        foreach (var obj in GetHoveredUiObjects(_currentRaycaster))
        {
            if(obj.TryGetComponent(out TooltipInfo tooltip))
            {
                ShowTooltip(tooltip.Text);
                return;
            }
        }
        
        HideTooltip();
    }
}