using RedLoader;
using SonsSdk;
using SUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SonsGameManager;

using static SUI.SUI;

public class ModManagerUi
{
    public const string MOD_INDICATOR_ID = "ModIndicatorPanel";
    public const string MOD_LIST_ID = "ModListPanel";

    private static readonly Color MainBgBlack = new(0, 0, 0, 0.8f);
    private static readonly Color PanelBg = ColorFromString("#111111");
    private static readonly Color ComponentBlack = new(0, 0, 0, 0.6f);

    private static readonly BackgroundDefinition CardButtonBg = new(
        "#222", 
        GetBackgroundSprite(EBackground.ShadowPanel),
        Image.Type.Sliced);

    private static readonly BackgroundDefinition RedBackground = new(
        "#fff",
        GetBackgroundSprite(EBackground.Sons),
        Image.Type.Simple);
    
    private static readonly Dictionary<string, ModCardData> ModCardDatas = new();

    internal static void Create()
    {
        _ = RegisterNewPanel(MOD_INDICATOR_ID)
                .Pivot(0)
                .Anchor(AnchorType.TopLeft)
                .Size(250, 60)
                .Position(20, -105)
                .Background(SpriteBackground400ppu, MainBgBlack, Image.Type.Sliced)
            - SLabel
                .RichText($"Loaded <color=#eb8f34>{SonsMod.RegisteredMods.Count}</color> {"Mod".MakePlural(SonsMod.RegisteredMods.Count)}")
                .FontColor(Color.white.WithAlpha(0.3f)).FontSize(18).Dock(EDockType.Fill).Alignment(TextAlignmentOptions.Center);

        var panel = RegisterNewPanel(MOD_LIST_ID)
            .Dock(EDockType.Fill).Background(BlurBackground, Color.white).OverrideSorting(100);

        var mainContainer = SContainer
            .Dock(EDockType.Fill)
            .Margin(300, 300, 100, 100)
            .Background(RedBackground)
            .Material(GameResources.GetMaterial(MaterialAssetMap.DarkGreyBackgroundTransparent));
        panel.Add(mainContainer);

        var vertical = SContainer
                           .Dock(EDockType.Fill)
                           .Vertical(10, "EC").Padding(58, 58, 150, 10)
                       - ModLoaderCard();

        var title = SLabel.Text("INSTALLED MODS")
            .FontColor("#444").Font(EFont.RobotoRegular)
            .PHeight(100).FontSize(32)
            .HFill().Position(null, -95)
            .FontSpacing(10);
        title.SetParent(mainContainer);
        
        vertical.SetParent(mainContainer);
        
        var exitButton = SBgButton
            .Text("x").Background(GetBackgroundSprite(EBackground.Round28), Image.Type.Sliced).Color(ColorFromString("#FF234B"), -0.25f)
            .Pivot(1, 1).Anchor(AnchorType.TopRight).Position(-60, -60)
            .Size(60, 60).Ppu(1.7f).Notify(Close);
        exitButton.SetParent(mainContainer);
        
        // additional wrapper container for the list to add padding
        var paddingContainer = SDiv.FlexHeight(1);
        paddingContainer.SetParent(vertical);
        
        var scroll = SScrollContainer
            .Dock(EDockType.Fill)
            //.Background(ComponentBlack, EBackground.RoundedStandard)
            .Margin(20, 20, 20, 40)
            .As<SScrollContainerOptions>();
        scroll.ContainerObject.Spacing(4);
        scroll.SetParent(paddingContainer);
        
        foreach (var mod in SonsMod.RegisteredMods)
        {
            var data = new ModCardData
            {
                Name = string.IsNullOrEmpty(mod.Manifest.Name) ? mod.ID : mod.Manifest.Name,
                Author = mod.Manifest.Author,
                Version = mod.Manifest.Version,
                ID = mod.ID,
                HasSettings = new Observable<bool>(false),
                CustomActions = mod.GetModPanelActions()
            };
            ModCardDatas.Add(mod.ID, data);
            scroll.Add(ModCard(data));
        }
        
        AddModsButton();

        panel.Active(false);
        
        SdkEvents.OnSonsSceneInitialized.Subscribe(_=>
        {
            panel.Active(false);
            TogglePanel(ModSettingsUi.MOD_SETTINGS_NAME, false);
        });
        
        ModSettingsUi.Create();
    }

    public static void AddModsButton()
    {
        AddButtonToTitleMenu(() => SMenuButton.Text("MODS").MWidth(200).Notify(OnShowMods), "MODS", 3);
        AddButtonToPauseMenu(() => SMenuButton.Text("MODS").MWidth(200).FlexWidth(-1).PWidth(-1).Notify(OnShowMods), "MODS");
    }

    private static SContainerOptions ModCard(ModCardData data)
    {
        SContainerOptions buttonHolder;
        var container = SContainer.Background(Color.black.WithAlpha(0.5f), EBackground.None).PHeight(100)
                        - SLabel.RichText($"<color=#BBB>{data.Name}</color> <color=#777>{data.Version}</color>").FontSize(22)
                            .Font(EFont.RobotoRegular)
                            .Dock(EDockType.Fill).Alignment(TextAlignmentOptions.MidlineLeft).Margin(50, 0, 0, 0)
                        - SLabel.Text(data.Author).Alignment(TextAlignmentOptions.MidlineRight).Pivot(1).Anchor(AnchorType.MiddleRight)
                            .FontSize(12).Position(-40).FontColor(Color.white.WithAlpha(0.3f))
                        - (buttonHolder = SContainer.Horizontal(2, "XE").LayoutChildAlignment(TextAnchor.MiddleRight).Dock(EDockType.Fill).Size(-500, -52));
        
        foreach (var (name, action) in data.CustomActions)
        {
            buttonHolder.Add(SBgButton.Background(CardButtonBg).Size(150, 45).Ppu(3)
                .Notify(action).Text(name).FontSize(12));
        }
        
        buttonHolder.Add(SBgButton.Background(CardButtonBg).Size(150, 45).Ppu(3)
            .Notify(data.OnSettingsClicked).Text("Settings").FontSize(12).BindVisibility(data.HasSettings));

        return container;
    }

    private static SContainerOptions ModLoaderCard()
    {
        return SContainer.Background(new Color(0.2353f, 0.2353f, 0.2353f, 0.0784f), EBackground.Round28, Image.Type.Sliced).PHeight(100)
               - SSprite.Sprite(GetSprite("red_logo")).Anchor(AnchorType.MiddleLeft).Pivot(0, 0.5f).Position(30, 2).Size(86, 28)
               - SLabel.Text($"{BuildInfo.Version}").FontSize(25).Font(EFont.RobotoLight)
                   .Dock(EDockType.Fill).Alignment(TextAlignmentOptions.MidlineLeft)
                   .FontColor(new Color(0.3f, 0.3f, 0.3f)).Margin(140, 2, 0, 0)
               - SLabel.Text("Toni Macaroni").Alignment(TextAlignmentOptions.MidlineRight).Pivot(1).Anchor(AnchorType.MiddleRight)
                   .FontSize(15).Position(-40).FontColor(Color.white.WithAlpha(0.3f));
    }

    private static void OnShowMods()
    {
        foreach (var data in ModCardDatas.Values)
        {
            data.HasSettings.Value = SettingsRegistry.HasSettings(data.ID);
        }

        TogglePanel(MOD_LIST_ID);
    }

    private static void Close()
    {
        TogglePanel(MOD_LIST_ID);
    }

    private class ModCardData
    {
        public string Author;
        public string Name;
        public string Version;
        public Observable<bool> HasSettings;
        public Dictionary<string, Action<SBgButtonOptions>> CustomActions;

        public string ID;

        public void OnSettingsClicked()
        {
            ModSettingsUi.Open(ID);
        }
    }
}