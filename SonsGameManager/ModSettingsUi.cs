using RedLoader;
using SonsSdk;
using SUI;
using UnityEngine;
using UnityEngine.UI;

namespace SonsGameManager;

using static SUI.SUI;

public class ModSettingsUi
{
    public const string MOD_SETTINGS_NAME = "ModSettingsPanel";

    private static readonly BackgroundDefinition MainBg = new(
        "#050505",
        GetBackgroundSprite(EBackground.Sons),
        Image.Type.Sliced);
    
    private static readonly BackgroundDefinition ButtonBg = new(
        ColorFromString("#191A23"), 
        GetBackgroundSprite(EBackground.Round10), 
        Image.Type.Sliced);

    private static readonly BackgroundDefinition PanelBg = new(
        ColorFromString("#111"), 
        GetBackgroundSprite(EBackground.Round10), 
        Image.Type.Sliced);
    
    private static SContainerOptions _currentSettingsPanel;
    
    private static bool _initialized;
    private static SScrollContainerOptions _mainContainer;
    
    private static readonly List<Transform> CurrentChildren = new();
    
    private static readonly Observable<string> Title = new("Mod Settings");

    public static void Create()
    {
        if (_initialized)
            return;

        _initialized = true;

        var panel = RegisterNewPanel(MOD_SETTINGS_NAME)
            .OverrideSorting(200)
            .Background(MainBg)
            .Dock(EDockType.Fill).Size(-400,-100);
        panel.Active(false);

        panel.Add(SLabel.Bind(Title).Dock(EDockType.Top)
            .Size(0, 60).Position(0, -30)
            .FontColor(Color.white.WithAlpha(0.3f)).Font(EFont.RobotoLight).FontSpacing(3));

        panel.Add(SContainer.Background(new (0.2f, 0.2f, 0.2f), EBackground.None)
            .Dock(EDockType.Top).Size(-100, 3).Position(0, -100));

        _mainContainer = SScrollContainer.Margin(10,10,120,100).As<SScrollContainerOptions>();
        panel.Add(_mainContainer);
        
        panel.Add(SBgButton
            .Background(ButtonBg).Background("#990508").Ppu(3)
            .Pivot(1, 0).Anchor(AnchorType.BottomRight).Position(-40, 40).Size(300, 60)
            .RichText("Exit " + SpriteText("arrow_right")).FontSize(20).Notify(Close));
    }

    public static void Open(SContainerOptions settingsPanel)
    {
        _currentSettingsPanel = settingsPanel;
        var t = _currentSettingsPanel.Root.transform;
        for (int i = 0; i < t.childCount; i++)
        {
            var child = t.GetChild(0);
            CurrentChildren.Add(child);
            child.SetParent(_mainContainer.ContainerObject.RectTransform);
        }

        Title.Value = "Mod Settings";

        TogglePanel(MOD_SETTINGS_NAME, true);
    }

    public static void Open(string id)
    {
        if (GetSettingsPanel(id) is { } panel)
        {
            RLog.Debug($"Found panel for {id}");
            Open(panel);
            Title.Value = $"Mod Settings [{id}]";
        }
    }

    public static void Close()
    {
        if (_currentSettingsPanel != null)
        {
            foreach (var child in CurrentChildren)
            {
                child.SetParent(_currentSettingsPanel.RectTransform);
            }
            CurrentChildren.Clear();
            _currentSettingsPanel = null;
        }
        
        TogglePanel(MOD_SETTINGS_NAME, false);
    }
}