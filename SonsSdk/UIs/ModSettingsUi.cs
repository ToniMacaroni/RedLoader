using RedLoader;
using Sons.Ai.Vail;
using SonsSdk;
using SUI;
using UnityEngine;
using UnityEngine.UI;

namespace SonsGameManager;

using static SUI.SUI;

public class ModSettingsUi
{
    public const string MOD_SETTINGS_NAME = "ModSettingsPanel";
    
    public static GraphicRaycaster Raycaster { get; private set; }

    private static readonly BackgroundDefinition MainBg = new(
        "#fff",
        GetBackgroundSprite(EBackground.Sons),
        Image.Type.Simple);
    
    private static readonly BackgroundDefinition ButtonBg = new(
        ColorFromString("#191A23"), 
        GetBackgroundSprite(EBackground.Round10), 
        Image.Type.Sliced);

    private static SettingsRegistry.SettingsEntry _currentEntry;
    
    private static bool _initialized;
    private static SScrollContainerOptions _mainContainer;
    
    private static readonly Observable<string> Title = new("Mod Settings");
    private static string _currentModId;

    public static void Create()
    {
        if (_initialized)
            return;

        _initialized = true;

        var panel = RegisterNewPanel(MOD_SETTINGS_NAME)
            .OverrideSorting(200)
            .Background(MainBg).Material(GameResources.GetMaterial(MaterialAssetMap.DarkGreyBackground))
            .Dock(EDockType.Fill).Size(-400,-100);
        panel.Active(false);
        
        Raycaster = panel.Root.GetComponent<GraphicRaycaster>();

        panel.Add(SLabel.Bind(Title).Dock(EDockType.Top)
            .Size(0, 60).Position(0, -30)
            .FontColor(Color.white.WithAlpha(0.3f)).Font(EFont.RobotoLight).FontSpacing(3).Active(false));

        // panel.Add(SContainer.Background(new (0.2f, 0.2f, 0.2f), EBackground.None)
        //     .Dock(EDockType.Top).Size(-100, 3).Position(0, -100));

        _mainContainer = SScrollContainer.Margin(100,100,100,160).As<SScrollContainerOptions>();
        _mainContainer.ContainerObject.Padding(2, 4, 0, 0);
        panel.Add(_mainContainer);
        
        panel.Add(SBgButton
            .Background(ButtonBg).Background("#990508").Ppu(3)
            .Pivot(0, 0).Anchor(AnchorType.BottomLeft).Position(80, 80).Size(300, 60)
            .RichText("\uF060 Back").FontColor("#ff5856").UpperCase().FontSize(20).Notify(Close));
        
        panel.Add(SBgButton
            .Background(ButtonBg).Background("#796C4E").Ppu(3)
            .Pivot(0, 0).Anchor(AnchorType.BottomLeft).Position(390, 80).Size(200, 60)
            .RichText("Revert \uf0e2").FontColor("#c3ba8b").UpperCase().FontSize(20).Notify(RevertSettings));
    }

    public static void Open(string id)
    {
        if (SettingsRegistry.GetEntry(id) is { } entry)
        {
            RLog.Debug($"Found panel for {id}");
            _currentModId = id;
            _currentEntry = entry;
            entry.ParentTo(_mainContainer.ContainerObject.RectTransform);
            Title.Value = $"MOD SETTINGS <color=#ea2f4e>[{id}]</color>";
            TogglePanel(MOD_SETTINGS_NAME, true);
            TooltipProvider.RaycastFor(Raycaster);
        }
    }

    public static void Close()
    {
        _currentEntry?.Unparent();
        _currentEntry = null;
        
        var showRestartPrompt = false;
        
        if (!string.IsNullOrEmpty(_currentModId) && SettingsRegistry.TryGetEntry(_currentModId, out var entry))
        {
            entry.Save();
            entry.InvokeCallbacks();
            showRestartPrompt = entry.ChangesNeedRestart && entry.CheckForChanges();
        }
        
        _currentModId = null;
        
        TogglePanel(MOD_SETTINGS_NAME, false);
        TooltipProvider.StopRaycasting();

        if (showRestartPrompt)
        {
            GenericModalDialog.ShowDialog("Restart", "You need to restart the game for the changes to take effect.").SetOption1("Ok", ()=>{});
        }
    }

    public static void RevertSettings()
    {
        if (_currentEntry == null)
        {
            return;
        }
        
        _currentEntry.RevertSettings();
    }
}
