using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Endnight.Utilities;
using Il2CppInterop.Runtime.Injection;
using JetAnnotations;
using JetBrains.Annotations;
using RedLoader;
using RedLoader.Utils;
using Sons.Gui;
using Sons.Gui.Options;
using Sons.Input;
using Sons.Loading;
using Sons.UiElements;
using SonsSdk;
using TheForest.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.PropertyVariants;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SUI;

public partial class SUI
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
    private static GameObject _menuButtonPrefab;
    private static GameObject _scrollContainerPrefab;
    private static GameObject _tabControllerPrefab;

    private static Transform _titleMenuButtonsContainer;

    private static Sprite _sonsBackgroundSprite;
    private static Sprite _roundBackgroundSprite;

    private static GameObject _eventSystemObject;
    
    private static List<MenuButtonRegistration> _registeredMenuButtons = new();

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
    
    public static SMenuButtonOptions SMenuButton => new(Object.Instantiate(_menuButtonPrefab));
    
    public static SMaskedImageOptions SMaskedImage => new(Object.Instantiate(_maskedImagePrefab));
    
    public static SSpriteOptions SSprite => new(new GameObject("Sprite"));
    
    public static SImageOptions SImage => new(new GameObject("Image"));
    
    public static SContainerOptions SContainer => new(new GameObject("Container"));

    public static SContainerOptions SDiv => new SContainerOptions(new GameObject("Container"))
        .Pivot(0,1)
        .Anchor(AnchorType.TopLeft);
    
    public static STabControllerOptions STabController => new(Object.Instantiate(_tabControllerPrefab));
    
    public static SScrollContainerOptions SScrollContainer => new(Object.Instantiate(_scrollContainerPrefab));

    public static SIconButtonOptions SIconButton => new(Object.Instantiate(_bgButtonPrefab));
    
    // ======= Sprite strong type =======
    public static Sprite SpriteBackground400ppu => GetSprite("Background (400ppu)");
    public static Sprite SpriteBackground => GetSprite("Background");
    public static Sprite BlurBackground => GetSprite("Blurred Title Screen");
    
    internal static void InitPrefabs()
    {
        var sw = TimingLogger.StartNew("SUI.InitPrefabs");
        
        if (IsInitialized)
            return;

        InitBundleContent();
        
        GetPrefabs(); // Gather all the prefabs
        BackupPrefabs(); // Create a copy of the prefabs
        PreparePrefabs(); // Modify the duplicated prefabs
        
        PrintSprites();
        
        SUIViewport = CreateViewport();
        
        IsInitialized = true;
        
        SdkEvents.OnSonsSceneInitialized.Subscribe(OnSonsScene);
        
        sw.Stop();
    }

    [Conditional("DEBUG")]
    private static void PrintSprites()
    {
        RLog.WriteLine(10);
        RLog.Debug("Sprites:");
        foreach (var (key, value) in GameResources.Sprites)
        {
            RLog.Debug(key);
        }
        RLog.WriteLine(10);
    }

    /// <summary>
    /// Gather all the prefabs
    /// </summary>
    private static void GetPrefabs()
    {
        var optionsPanel = Resources.FindObjectsOfTypeAll<OptionsGuiManager>().FirstWithName("OptionsPanel");
        var displayOptions = optionsPanel._optionGroups._items.FirstWithName("DisplayPanel");
        var gameplayOptions = optionsPanel._optionGroups._items.FirstWithName("GameplayPanel");
        var prefabDialog = ModalDialogManager._instance.transform.Find("DynamicModalDialogGui/Panel");

        _sonsBackgroundSprite = SpriteBackground400ppu;
        _roundBackgroundSprite = SpriteBackground;

        _sliderPrefab = displayOptions.Get<TargetFrameRateOptionGui>()._optionGuiRoot;
        _optionsPrefab = displayOptions.Get<FullscreenOptionGui>()._optionGuiRoot;
        _labelDividerPrefab = gameplayOptions.Get<FovOffsetOptionGui>()._optionGuiRoot.transform.parent.parent.Find("LabelPanel").gameObject;
        _textPrefab = prefabDialog.Find("Content").gameObject;
        _inputPrefab = prefabDialog.Find("InputField").gameObject;
        _buttonPrefab = prefabDialog.Find("ButtonsLayout/BackButton").gameObject;
        _scrollContainerPrefab = _labelDividerPrefab.transform.parent.parent.parent.parent.parent.gameObject;
        
        foreach (var button in Resources.FindObjectsOfTypeAll<Button>())
        {
            var parent = button.transform.parent;
            if(parent && parent.name == "PerformanceRaterGui")
                _bgButtonPrefab = button.gameObject;
            else if (button.name == "SinglePlayerButton")
            {
                _menuButtonPrefab = button.gameObject;
                _titleMenuButtonsContainer = button.transform.parent;
            }
        }
        
        _togglePrefab = CreateTogglePrefab();
        _maskedImagePrefab = CreateMaskedImagePrefab();
        _tabControllerPrefab = CreateTabControllerPrefab();
    }

    /// <summary>
    /// Create a copy of the prefabs
    /// </summary>
    private static void BackupPrefabs()
    {
        CheckForNull(_sonsBackgroundSprite, nameof(_sonsBackgroundSprite));
        CheckForNull(_roundBackgroundSprite, nameof(_roundBackgroundSprite));
        CheckForNull(_sliderPrefab, nameof(_sliderPrefab));
        CheckForNull(_optionsPrefab, nameof(_optionsPrefab));
        CheckForNull(_textPrefab, nameof(_textPrefab));
        CheckForNull(_labelDividerPrefab, nameof(_labelDividerPrefab));
        CheckForNull(_togglePrefab, nameof(_togglePrefab));
        CheckForNull(_inputPrefab, nameof(_inputPrefab));
        CheckForNull(_buttonPrefab, nameof(_buttonPrefab));
        CheckForNull(_bgButtonPrefab, nameof(_bgButtonPrefab));
        CheckForNull(_maskedImagePrefab, nameof(_maskedImagePrefab));
        CheckForNull(_menuButtonPrefab, nameof(_menuButtonPrefab));
        CheckForNull(_scrollContainerPrefab, nameof(_scrollContainerPrefab));

        // Create a copy so we can access them from anywhere
        // and the state can't be modified from outside
        _sliderPrefab = TryBackup(_sliderPrefab);
        _optionsPrefab = TryBackup(_optionsPrefab);
        _textPrefab = TryBackup(_textPrefab);
        _labelDividerPrefab = TryBackup(_labelDividerPrefab);
        _inputPrefab = TryBackup(_inputPrefab);
        _buttonPrefab = TryBackup(_buttonPrefab);
        _bgButtonPrefab = TryBackup(_bgButtonPrefab);
        _menuButtonPrefab = TryBackup(_menuButtonPrefab);
        _scrollContainerPrefab = TryBackup(_scrollContainerPrefab);
    }

    /// <summary>
    /// Cleanup and modify the duplicated prefabs
    /// </summary>
    private static void PreparePrefabs()
    {
        // === SSLIDER ===
        {
            _sliderPrefab.FindGet<TextMeshProUGUI>("LabelPanel/Label").gameObject.Destroy<LocalizeStringEvent>();
            _sliderPrefab.SetActive(false);
        }
        
        // === SOPTIONS ===
        {
            var dropdownObject = _optionsPrefab.FindGet<SonsDropdown>("DropdownPanel/Dropdown");
            var textObject = _optionsPrefab.FindGet<TextMeshProUGUI>("LabelPanel/Label");
            textObject.gameObject.Destroy<LocalizeStringEvent>();

            dropdownObject.gameObject.Destroy<GameObjectLocalizer>();

            dropdownObject.ClearOptions();
            dropdownObject.m_Options.options.Clear();
            dropdownObject.options.Clear();
            _optionsPrefab.SetActive(false);
        }
        
        // === SDIVIDER ===
        {
            _labelDividerPrefab.FindGet<TextMeshProUGUI>("ScreenLabel").gameObject.Destroy<LocalizeStringEvent>();
            _labelDividerPrefab.SetActive(false);
        }
        
        // === SLABEL ===
        {
            var textObject = _textPrefab.GetComponent<TextMeshProUGUI>();
            textObject.gameObject.Destroy<LocalizeStringEvent>();
            textObject.gameObject.Destroy<LayoutElement>();
            textObject.gameObject.Destroy<ContentSizeFitter>();
            textObject.margin = new Vector4(0, 0, 0, 0);
            textObject.enableWordWrapping = false;
            textObject.fontSizeMin = 0;
            textObject.fontSizeMax = 60;
            textObject.enableAutoSizing = false;
            textObject.alignment = TextAlignmentOptions.Center;
            _textPrefab.SetActive(false);
        }
        
        // === STEXTBOX ===
        {
            var inputFieldObject = _inputPrefab.FindGet<TMP_InputField>("InputPanel/InputField");
            var placeholderObject = inputFieldObject.placeholder.GetComponent<TextMeshProUGUI>();
            inputFieldObject.gameObject.SetActive(true);
            var textObject = _inputPrefab.FindGet<TextMeshProUGUI>("Label");
            textObject.gameObject.Destroy<LocalizeStringEvent>();
            textObject.gameObject.SetActive(true);

            var horizontal = _inputPrefab.GetComponent<HorizontalLayoutGroup>();
            horizontal.padding = new RectOffset(0, 0, 0, 0);
            horizontal.spacing = 0;
            horizontal.childForceExpandWidth = true;
        
            placeholderObject.color = new Color(1,1,1,0.2f);

            textObject.enableAutoSizing = false;
            textObject.fontSize = 20;

            _inputPrefab.SetActive(false);
        }
        
        // === SBUTTON ===
        {
            var textObject = _buttonPrefab.FindGet<TextMeshProUGUI>("ContentPanel/TextBase");
            textObject.fontSize = 30;
            textObject.text = "Button";
            textObject.margin = new Vector4(0, 0, 0, 0);

            _buttonPrefab.Destroy<LocalizeStringEvent>();
            _buttonPrefab.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            _buttonPrefab.SetActive(false);
        }
        
        // === SSCROLLCONTAINER ===
        var scrollContainer = _scrollContainerPrefab.transform.Find("Viewport/Content");

        for (int i = 0; i < scrollContainer.childCount; i++)
        {
            Object.DestroyImmediate(scrollContainer.GetChild(0).gameObject);
        }
        
        _scrollContainerPrefab.name = "SUI_ScrollContainer";
    }

    private static void OnSonsScene(ESonsScene scene)
    {
        switch (scene)
        {
            case ESonsScene.Title:
                _titleMenuButtonsContainer = Resources.FindObjectsOfTypeAll<Button>().First(x=>x.name == "SinglePlayerButton" && x.gameObject.activeSelf).transform.parent;
                break;
        }

        foreach (var button in _registeredMenuButtons)
        {
            if (scene == ESonsScene.Title && button.Scene == ESonsScene.Title)
            {
                AddButtonToBottomCanvas(button);
            }
        }
    }

    /// <inheritdoc cref="RegisterNewPanel(string,bool,System.Nullable{UnityEngine.KeyCode})"/>
    public static SPanelOptions RegisterNewPanel(string id, bool enableInput = false, KeyCode? toggleKey = null)
    {
        return RegisterNewPanel(id, null, enableInput, toggleKey);
    }

    /// <summary>
    /// Creates a new panel and registers it to the sui system.
    /// </summary>
    /// <param name="id">The id by which you can manage the panel later. Needs to be unique</param>
    /// <param name="parent">The transform to parent the panel to</param>
    /// <param name="enableInput">If true enables the mouse and disables game keyboard input once the panel is showing</param>
    /// <param name="toggleKey">Optional key by which you can toggle the panel</param>
    /// <returns></returns>
    public static SPanelOptions RegisterNewPanel(string id, Transform parent, bool enableInput = false, KeyCode? toggleKey = null)
    {
        var panel = CreatePanel(parent);
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
        
        if (toggleKey != null)
        {
            if (!GlobalInput.RegisterKey(toggleKey.Value, () => panel.Toggle()))
            {
                RLog.Error("Key already registered: " + toggleKey.Value);
            }
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
    
    public static bool RemovePanel(string id)
    {
        var panel = GetPanel(id);
        if (panel == null)
            return false;
        _panels.Remove(id);
        panel.Remove();
        return true;
    }

    internal static void OnPauseMenuCreated(PauseMenu menu)
    {
        var buttonHub = menu.transform.Find("Panel/BottomMenuPanel/LeftPanel");
        var buttons = buttonHub.GetChildren().Select(x => x.name).ToHashSet();

        foreach (var button in _registeredMenuButtons.Where(x=>x.Scene == ESonsScene.Game))
        {
            if(!buttons.Contains(button.Id))
            {
                var go = button.ElementFactory();
                go.Root.name = button.Id;
                go.SetParent(buttonHub);
                if(button.Index != -1)
                    go.RectTransform.SetSiblingIndex(button.Index);
                
                menu._activateForMenu.Add(go.Root);
            }
        }
    }

    public static void AddButtonToPauseMenu(Func<SUiElement> generator, string id, int index = -1)
    {
        var button = new MenuButtonRegistration(generator, id, ESonsScene.Game, index);
        
        _registeredMenuButtons.Add(button);
    }
    
    public static void AddButtonToTitleMenu(Func<SUiElement> generator, string id, int index = -1)
    {
        var button = new MenuButtonRegistration(generator, id, ESonsScene.Title, index);
        
        if (SceneManager.GetActiveScene().name == SonsSceneManager.TitleSceneName)
        {
            AddButtonToBottomCanvas(button);
        }

        _registeredMenuButtons.Add(button);
    }

    private static void AddButtonToBottomCanvas(MenuButtonRegistration button)
    {
        if(_titleMenuButtonsContainer.Find(button.Id) != null)
            return;

        var element = button.ElementFactory();

        element.Root.name = button.Id;
        element.SetParent(_titleMenuButtonsContainer);
        RLog.Debug("Added button to title menu");
        if(button.Index != -1)
            element.Root.transform.SetSiblingIndex(button.Index);
    }

    private static void CreateTestUi()
    {
        var flareGunIcon = AssetLoaders.LoadAsset<Sprite>("FlareGunIcon");
        
        void OnSliderChanged(float val)
        {
            RLog.Msg(System.Drawing.Color.Aquamarine, $"Slider: {val}");
        }

        void OnOptionChanged(string val)
        {
            RLog.Msg(System.Drawing.Color.Aquamarine, $"Option: {val}");
        }
        
        void OnOkClicked()
        {
            RLog.Msg(System.Drawing.Color.Aquamarine, "Ok clicked");
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

        rootBoi.Add(SHorizontal.LayoutMode("EC").AutoSize("UM")
                    - IconButton("Toni", flareGunIcon, () => RLog.Msg(System.Drawing.Color.Aquamarine, "Toni clicked"))
                    - IconButton("Macaroni", flareGunIcon, () => RLog.Msg(System.Drawing.Color.Aquamarine, "Macaroni clicked")));

        rootBoi.Add(SHorizontal
                    - SButton.Text("Ok").Notify(OnOkClicked)
                    - SButton.Text("Cancel"));
    }
    
    public static SContainerOptions IconButton(string title, Sprite icon, Action onClick)
    {
        return SVertical.Background(BG_CYAN, EBackground.None).LayoutMode("EC").PaddingVertical(25).Spacing(40).OnClick(onClick)
               - (SContainer.MHeight(100) - SSprite.Anchor(AnchorType.FillVertical).Size(100, 0).Sprite(icon))
               - SLabel.MHeight(20).Text(title);
    }

    public static Sprite GetBackgroundSprite(EBackground type) => type switch
        {
            EBackground.Sons => _sonsBackgroundSprite,
            EBackground.RoundedStandard => _roundBackgroundSprite,
            EBackground.Round8 => GetSprite("RoundRect8"),
            EBackground.Round10 => GetSprite("RoundRect10"),
            EBackground.RoundSmall => GetSprite("RoundRectSmall"),
            EBackground.Round28 => GetSprite("RoundRect28"),
            EBackground.RoundNormal => GetSprite("RoundRectNormal"),
            EBackground.RoundOutline => GetSprite("RectOutline"),
            EBackground.RoundOutline10 => GetSprite("RoundRect10Outline"),
            EBackground.ShadowPanel => GetSprite("ShadowPanel"),
            _ => null
        };

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
        if(GameResources.Sprites.TryGetValue(name, out var sprite))
            return sprite;
        
        RLog.Debug("Did not find sprite: " + name);
        return null;
    }

    public static Color ColorFromString(string color)
    {
        if (string.IsNullOrEmpty(color))
            return Color.white;

        if (color.StartsWith("#"))
            color = color.Substring(1);

        if (color.Length == 3 || color.Length == 4) 
        {
            var rChar = color[0];
            var gChar = color[1];
            var bChar = color[2];
            var aChar = color.Length == 4 ? color[3] : 'F';
            color = $"{rChar}{rChar}{gChar}{gChar}{bChar}{bChar}{aChar}{aChar}";
        }
        else if (color.Length != 6 && color.Length != 8)
        {
            return Color.white;
        }

        var r = Convert.ToInt32(color.Substring(0, 2), 16);
        var g = Convert.ToInt32(color.Substring(2, 2), 16);
        var b = Convert.ToInt32(color.Substring(4, 2), 16);
        var a = color.Length == 8 ? Convert.ToInt32(color.Substring(6, 2), 16) : 255;

        return new Color((float)r/255, (float)g/255, (float)b/255, (float)a/255);
    }

    private static void CheckForNull(Object obj, string memberName)
    {
        if (!obj)
        {
            RLog.Error($"= {memberName} is null! =");
        }
    }

    private static GameObject TryBackup(GameObject go)
    {
        return Object.Instantiate(go).DontDestroyOnLoad().HideAndDontSave();
    }

    private static GameObject CreateMaskedImagePrefab()
    {
        var rootgo = new GameObject("MaskedImage");
        rootgo.AddComponent<RectTransform>();
        var rawImage = rootgo.AddComponent<RawImage>();
        rawImage.texture = GetSprite("Background (400ppu)").texture;
        var mask = rootgo.AddComponent<UnityEngine.UI.Mask>();
        mask.showMaskGraphic = false;
        
        var imageGo = new GameObject("Image");
        imageGo.transform.SetParent(rootgo.transform);
        imageGo.AddComponent<RectTransform>();
        imageGo.AddComponent<RawImage>();

        return rootgo.DontDestroyOnLoad().HideAndDontSave();
    }

    private static GameObject CreateTogglePrefab()
    {
        var toggleGameObject = new GameObject("Toggle");

        var toggle = toggleGameObject.AddComponent<Toggle>();

        SLabel
            .Text("Toggle")
            .Dock(EDockType.Fill)
            .Alignment(TextAlignmentOptions.MidlineLeft)
            .Margin(0,0,0,0)
            .Name("Label")
            .SetParent(toggleGameObject.transform);
        
        var background = SDiv
            .Anchor(AnchorType.MiddleRight)
            .Size(42, 42)
            .Pivot(1, 0.5f)
            .Position(-20, 0)
            .Background(SpriteBackground400ppu, ColorFromString("#333"), Image.Type.Tiled).Ppu(5);
        background.SetParent(toggleGameObject.transform);

        var checkmark = SDiv
            .Dock(EDockType.Fill)
            .Size(-15, -15)
            .Pivot(0.5f, 0.5f)
            .Background(new Color(1,1,1,0.3f), EBackground.None);
        checkmark.SetParent(background);

        toggle.graphic = checkmark.Root.GetComponent<Image>();
        toggle.targetGraphic = background.Root.GetComponent<Image>();
        
        return toggleGameObject.DontDestroyOnLoad().HideAndDontSave();
    }

    private static GameObject CreateTabControllerPrefab()
    {
        var tabPanel = SDiv.Name("TabController").Vertical(0, "EC");

        var tabHeader = SContainer.PHeight(50).Horizontal(10,"XC").Name("TabHeader").Padding(10);
        tabPanel.Add(tabHeader);
        tabPanel.Add(SContainer.Background(Color.black).PHeight(5).Name("TabDivider"));
        var tabContent = SContainer.FlexHeight(1).Name("TabContent");
        tabPanel.Add(tabContent);

        return tabPanel.Root.DontDestroyOnLoad().HideAndDontSave();
    }

    public static TMP_FontAsset GetFont(EFont font) => font switch
    {
        EFont.RobotoDefault => GameResources.Fonts["Roboto-Regular SDF"],
        EFont.NotoSans => GameResources.Fonts["NotoSansTC-Regular SDF"],
        EFont.RobotoBold => GameResources.Fonts["RobotoCondensed-Bold SDF"],
        EFont.RobotoBlur => GameResources.Fonts["RobotoCondensed-Bold SDFBlur"],
        EFont.Montserrat => GameResources.Fonts["Montserrat-Medium SDF"],
        EFont.RobotoRegular => GameResources.Fonts["RobotoCondensed-Regular SDF"],
        EFont.RobotoLight => GameResources.Fonts["RobotoCondensed-Light SDF"],
        EFont.FatDebug => GameResources.Fonts["VailDebugFont"],
        _ => throw new ArgumentOutOfRangeException(nameof(font), font, null)
    };

    /// <summary>
    /// Get a rich text string for a sprite
    /// </summary>
    /// <param name="spriteName">Name of the sprite</param>
    /// <param name="color">Optional color of the sprite. White by default</param>
    /// <returns>A string in the form of &lt;sprite name="sprite" color=#000&gt;</returns>
    public static string SpriteText(string spriteName, string color = null)
    {
        if (color == null)
            return $"<sprite name=\"{spriteName}\">";
        
        return $"<sprite name=\"{spriteName}\" color={color}>";
    }
    
    /// <summary>
    /// Wraps some text in a rich text color tag
    /// </summary>
    /// <param name="text">The text to wrap</param>
    /// <param name="color">The color of the text</param>
    /// <returns>A string in the form of &lt;color=#000&gt;text&lt;/color&gt;</returns>
    public static string WrapColor(string text, string color)
    {
        return $"<color={color}>{text}</color>";
    }

    private struct MenuButtonRegistration
    {
        public readonly Func<SUiElement> ElementFactory;
        public readonly string Id;
        public readonly ESonsScene Scene;
        public readonly int Index;
        
        public MenuButtonRegistration(Func<SUiElement> elementFactory, string id, ESonsScene scene, int index)
        {
            ElementFactory = elementFactory;
            Id = id;
            Scene = scene;
            Index = index;
        }
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
            RLog.Msg("Enabling event system");
            SetEventSystemActive(true);
        }
        
        private void OnDisable()
        {
            ActiveEnablers--;
            if(ActiveEnablers == 0)
            {
                RLog.Msg("No more active enablers, disabling event system");
                SetEventSystemActive(false);
            }
        }
    }

    public struct BackgroundDefinition
    {
        public Color Color;
        public Sprite Sprite;
        public Image.Type Type;
        
        public BackgroundDefinition(Color color, Sprite sprite, Image.Type type)
        {
            Color = color;
            Sprite = sprite;
            Type = type;
        }
        
        public BackgroundDefinition(string color, Sprite sprite, Image.Type type)
        {
            Color = ColorFromString(color);
            Sprite = sprite;
            Type = type;
        }
        
        public void ApplyTo(Image image)
        {
            image.color = Color;
            image.sprite = Sprite;
            image.type = Type;
        }
    }

    public enum EFont
    {
        RobotoDefault,
        NotoSans,
        RobotoBold,
        RobotoBlur,
        Montserrat,
        RobotoRegular,
        RobotoLight,
        FatDebug
    }
}