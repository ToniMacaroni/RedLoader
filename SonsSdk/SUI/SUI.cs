using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ForestNanosuit;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using MelonLoader.Utils;
using Sons.Gui;
using Sons.Gui.Options;
using Sons.Input;
using SonsSdk;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SUI;

public class SUI
{
    public static readonly Color BG_CYAN = new(0, 0.5f, 0.5f, 0.2f);

    public static Canvas SUIViewport { get; private set; }
    public static bool IsInitialized { get; private set; }
    
    public static Dictionary<string, SPanelOptions> _panels = new();

    private static GameObject _sliderPrefab;
    private static GameObject _optionsPrefab;
    private static GameObject _textPrefab;
    private static GameObject _labelDividerPrefab;
    private static GameObject _togglePrefab;
    private static GameObject _inputPrefab;
    private static GameObject _buttonPrefab;
    private static GameObject _bgButtonPrefab;
    private static GameObject _maskedImagePrefab;

    private static Sprite _sonsBackgroundSprite;
    private static Sprite _roundBackgroundSprite;

    private static GameObject _eventSystemObject;

    public static SSliderOptions SSlider => new(Object.Instantiate(_sliderPrefab));

    public static SOptionsOptions SOptions => new(Object.Instantiate(_optionsPrefab));

    public static SLabelOptions SLabel => new(Object.Instantiate(_textPrefab));

    public static SLabelDividerOptions SLabelDivider => new(Object.Instantiate(_labelDividerPrefab));

    public static SToggleOptions SToggle => new(Object.Instantiate(_togglePrefab));

    public static SContainerOptions SBackground
    {
        get
        {
            var obj = new SContainerOptions(new GameObject("Background"));
            obj.Background(Color.black);
            return obj;
        }
    }
    
    public static SContainerOptions SHorizontal
    {
        get
        {
            var obj = new SContainerOptions(new GameObject("HorizontalContainer"));
            obj.Horizontal();
            return obj;
        }
    }
    
    public static SContainerOptions SVertical
    {
        get
        {
            var obj = new SContainerOptions(new GameObject("VerticalContainer"));
            obj.Vertical();
            return obj;
        }
    }

    public static STextboxOptions STextbox => new(Object.Instantiate(_inputPrefab));
    
    public static SButtonOptions SButton => new(Object.Instantiate(_buttonPrefab));
    
    public static SBgButtonOptions SBgButton => new(Object.Instantiate(_bgButtonPrefab));
    
    public static SMaskedImageOptions SMaskedImage => new(Object.Instantiate(_maskedImagePrefab));
    
    public static SSpriteOptions SSprite => new(new GameObject("Sprite"));
    
    public static SImageOptions SImage => new(new GameObject("Image"));
    
    public static SContainerOptions SContainer => new(new GameObject("Container"));
    
    private static Dictionary<string, Sprite> _sprites = new();

    public static void InitPrefabs()
    {
        var sw = TimingLogger.StartNew("SUI.InitPrefabs");
        
        if (IsInitialized)
            return;
        //var uiprefab = AssetLoaders.LoadPrefab("testicalui").transform;

        var optionsPanel = Resources.FindObjectsOfTypeAll<OptionsGuiManager>().FirstWithName("OptionsPanel");
        
        var displayOptions = optionsPanel._optionGroups._items.FirstWithName("DisplayPanel");
        var gameplayOptions = optionsPanel._optionGroups._items.FirstWithName("GameplayPanel");

        var prefabDialog = ModalDialogManager._instance.transform.Find("DynamicModalDialogGui/Panel");

        _sonsBackgroundSprite = prefabDialog.GetComponent<Image>().sprite;
        _sliderPrefab = displayOptions.Get<TargetFrameRateOptionGui>()._optionGuiRoot;
        _optionsPrefab = displayOptions.Get<FullscreenOptionGui>()._optionGuiRoot;
        _labelDividerPrefab = gameplayOptions.Get<FovOffsetOptionGui>()._optionGuiRoot.transform.parent.parent.Find("LabelPanel").gameObject;
        _textPrefab = prefabDialog.Find("Content").gameObject;
        //_togglePrefab = uiprefab.Find("Toggle").gameObject;
        _inputPrefab = prefabDialog.Find("InputField").gameObject;
        _buttonPrefab = prefabDialog.Find("ButtonsLayout/BackButton").gameObject;
        //_maskedImagePrefab = uiprefab.Find("MaskedImage").gameObject;
        _roundBackgroundSprite = Resources.FindObjectsOfTypeAll<InputActiveTester>().First().transform.Find("Canvas/Panel").GetComponent<Image>()
            .sprite;
        
        _bgButtonPrefab = Resources.FindObjectsOfTypeAll<Button>().First(x=>x.transform.parent.name == "PerformanceRaterGui").gameObject;
        _sprites = Resources.FindObjectsOfTypeAll<Sprite>().ToDictionary(x => x.name, x => x);

        SUIViewport = CreateViewport();
        
        IsInitialized = true;
        
        sw.Stop();
    }

    /// <summary>
    /// Creates a new panel and registers it to the sui system.
    /// </summary>
    /// <param name="id">The id by which you can manager the panel later. Needs to be unique</param>
    /// <param name="enableInput">If true enables the mouse and disables game keyboard input once the panel is showing</param>
    /// <returns></returns>
    public static SPanelOptions RegisterNewPanel(string id, bool enableInput = false)
    {
        var panel = CreatePanel();
        panel.Id = id;
        panel.Root.name = id;
        _panels[id] = panel;

        if (enableInput)
        {
            var inputCursorState = panel.Root.AddComponent<InputCursorState>();
            inputCursorState._enabled = true;
            inputCursorState._hardwareCursor = true;
            inputCursorState._priority = 100;

            var inputActionMapState = panel.Root.AddComponent<InputActionMapState>();
            inputActionMapState._applyState = InputState.Console;
            
            panel.Root.AddComponent<EventSystemEnabler>();
        }

        return panel;
    }
    
    public static SPanelOptions GetPanel(string id)
    {
        if(_panels.TryGetValue(id, out var panel))
            return panel;
        return null;
    }

    public static bool TogglePanel(string id)
    {
        var panel = GetPanel(id);
        if (panel == null)
            return false;
        panel.Toggle();
        return panel.Root.activeSelf;
    }
    
    public static SPanelOptions TogglePanel(string id, bool show)
    {
        var panel = GetPanel(id);
        if (panel == null)
            return null;
        panel.Active(show);
        return panel;
    }
    
    public static bool ToggleMenuPanel(string id)
    {
        var newState = TogglePanel(id);
        SonsTools.MenuMode(newState);
        return newState;
    }
    
    public static SPanelOptions ToggleMenuPanel(string id, bool show)
    {
        var panel = TogglePanel(id, show);
        if(panel != null)
            SonsTools.MenuMode(show);
        return panel;
    }

    private static void CreateTestUi()
    {
        var flareGunIcon = AssetLoaders.LoadAsset<Sprite>("FlareGunIcon");
        
        void OnSliderChanged(float val)
        {
            MelonLogger.Msg(System.Drawing.Color.Aquamarine, $"Slider: {val}");
        }

        void OnOptionChanged(string val)
        {
            MelonLogger.Msg(System.Drawing.Color.Aquamarine, $"Option: {val}");
        }
        
        void OnOkClicked()
        {
            MelonLogger.Msg(System.Drawing.Color.Aquamarine, "Ok clicked");
        }

        var myText = new Observable<string>("hello");

        var rootBoi = CreatePanel().Dock(EDockType.Left).Width(600).Vertical().Padding(20).LayoutMode("EX").Background(new Color(0,0,0,0.6f), EBackground.None)
                      - SLabelDivider.Text("Custom").FontSize(24).FontColor(Color.cyan)
                      - SSlider.Text("Steps").Range(1, 10).IntStep().Value(4).Notify(OnSliderChanged)
                      - SOptions.Text("Mode").Options("Low", "Mid", "High", "Crazy", "Godlike").Value("Crazy").Notify(OnOptionChanged)
                      - STextbox.Text("Custom Mode").Placeholder("Type something...").Bind(myText)
                      - SToggle.Text("Enable something 1")
                      - SToggle.Text("Enable something 2").Value(false);

        rootBoi.Add(SHorizontal
                    - SLabel.Text("Hello")
                    - SLabel.Text("World")
                    - SLabel.Text("Hello"));

        rootBoi.Add(SLabel.Bind(myText));

        SContainerOptions IconButton(string title, Sprite icon, Action onClick)
        {
            return SVertical.Background(BG_CYAN, EBackground.None).LayoutMode("EC").PaddingVertical(25).Spacing(40).OnClick(onClick)
                   - (SContainer.MHeight(100) - SSprite.Anchor(AnchorType.FillVertical).Size(100, 0).Sprite(icon))
                   - SLabel.MHeight(20).Text(title);
        }

        rootBoi.Add(SHorizontal.LayoutMode("EC").AutoSize("UM")
                    - IconButton("Toni", flareGunIcon, () => MelonLogger.Msg(System.Drawing.Color.Aquamarine, "Toni clicked"))
                    - IconButton("Macaroni", flareGunIcon, () => MelonLogger.Msg(System.Drawing.Color.Aquamarine, "Macaroni clicked")));

        rootBoi.Add(SHorizontal
                    - SButton.Text("Ok").Notify(OnOkClicked)
                    - SButton.Text("Cancel"));
    }

    public static Sprite GetBackgroundSprite(EBackground type)
    {
        return type switch
        {
            EBackground.Sons => _sonsBackgroundSprite,
            EBackground.Rounded => _roundBackgroundSprite,
            _ => null
        };
    }

    public static SPanelOptions CreatePanel(Transform parent = null)
    {
        var rootGo = new GameObject("Root");
        rootGo.AddComponent<RectTransform>();
        var panel = new SPanelOptions(rootGo);
        panel.Anchor(AnchorType.Fill);
        
        if (!parent)
        {
            parent = SUIViewport.transform;
        }
        
        panel.SetParent(parent);
        
        return panel;
    }

    private static Canvas CreateViewport()
    {
        var go = new GameObject("SUICanvas");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var canvasScaler = go.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 1;
        canvasScaler.referencePixelsPerUnit = 100;

        go.AddComponent<GraphicRaycaster>();
        
        _eventSystemObject = new GameObject("EventSystem");
        _eventSystemObject.transform.SetParent(go.transform);
        _eventSystemObject.SetActive(false);
        _eventSystemObject.AddComponent<EventSystem>();
        _eventSystemObject.AddComponent<StandaloneInputModule>();
        _eventSystemObject.AddComponent<BaseInput>();

        go.DontDestroyOnLoad();
        
        return canvas;
    }
    
    internal static void SetEventSystemActive(bool active)
    {
        if (_eventSystemObject)
            _eventSystemObject.SetActive(active);
    }

    public static Sprite GetSprite(string name)
    {
        if(_sprites.TryGetValue(name, out var sprite))
            return sprite;
        return null;
    }
    
    public static Color ColorFromString(string color)
    {
        if (string.IsNullOrEmpty(color))
            return Color.white;
        if (color.StartsWith("#"))
            color = color.Substring(1);
        if (color.Length != 6)
            return Color.white;
        var r = Convert.ToInt32(color.Substring(0, 2), 16);
        var g = Convert.ToInt32(color.Substring(2, 2), 16);
        var b = Convert.ToInt32(color.Substring(4, 2), 16);
        return new Color(r, g, b);
    }

    private class EventSystemEnabler : MonoBehaviour
    {
        private static int ActiveEnablers = 0;
        
        static EventSystemEnabler()
        {
            ClassInjector.RegisterTypeInIl2Cpp<EventSystemEnabler>();
        }
        
        private void OnEnable()
        {
            ActiveEnablers++;
            MelonLogger.Msg("Enabling event system");
            SetEventSystemActive(true);
        }
        
        private void OnDisable()
        {
            ActiveEnablers--;
            if(ActiveEnablers == 0)
            {
                MelonLogger.Msg("No more active enablers, disabling event system");
                SetEventSystemActive(false);
            }
        }
    }
}