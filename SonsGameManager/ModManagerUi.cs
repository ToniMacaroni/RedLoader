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

    internal static void Create()
    {
        _ = RegisterNewPanel(MOD_INDICATOR_ID)
                .Pivot(0)
                .Anchor(AnchorType.MiddleLeft)
                .Size(250, 60)
                .Position(10, -300)
                .Background(SpriteBackground400ppu, MainBgBlack, Image.Type.Sliced)
            - SLabel
                .RichText($"Loaded <color=#eb8f34>{SonsMod.RegisteredMods.Count}</color> {"Mod".MakePlural(SonsMod.RegisteredMods.Count)}")
                .FontColor(Color.white.WithAlpha(0.3f)).FontSize(18).Dock(EDockType.Fill).Alignment(TextAlignmentOptions.Center);

        var panel = RegisterNewPanel(MOD_LIST_ID)
            .Dock(EDockType.Fill)
            .Margin(400, 400, 100, 200)
            .Background(PanelBg.WithAlpha(0.99f), EBackground.RoundSmall)
            .OverrideSorting(100);
        
        var vertical = SContainer
                           .Dock(EDockType.Fill)
                           .Vertical(10, "EC").Padding(2)
                       - SLabel.Text("Installed Mods").Font(EFont.FatDebug).PHeight(100).FontSize(30).FontSpacing(8.5f)
                       - ModLoaderCard();
        
        vertical.SetParent(panel);

        // additional wrapper container for the list to add padding
        var paddingContainer = SDiv.FlexHeight(1);
        paddingContainer.SetParent(vertical);
        
        var scroll = SScrollContainer
            .Dock(EDockType.Fill)
            .Background(ComponentBlack, EBackground.RoundedStandard)
            .Size(-20, -20)
            .As<SScrollContainerOptions>();
        scroll.ContainerObject.Spacing(10);
        scroll.SetParent(paddingContainer);
        
        foreach (var mod in SonsMod.RegisteredMods)
        {
            scroll.Add(ModCard(new ModCardData
            {
                Name = string.IsNullOrEmpty(mod.Manifest.Name) ? mod.ID : mod.Manifest.Name,
                Author = mod.Manifest.Author,
                Version = mod.Manifest.Version,
                ID = mod.ID
            }));
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
        AddToTitleMenuButton(() => SMenuButton.Text("MODS").MWidth(200).Notify(OnShowMods), "MODS", 3);
    }

    private static SContainerOptions ModCard(ModCardData data)
    {
        return SContainer.Background(Color.black.WithAlpha(0.5f), EBackground.None).PHeight(100)
               - SLabel.RichText($"<color=#BBB>{data.Name}</color> <color=#777>{data.Version}</color>").FontSize(22).Font(EFont.RobotoRegular)
                   .Dock(EDockType.Fill).Alignment(TextAlignmentOptions.MidlineLeft).Margin(50, 0, 0, 0)
               - SLabel.Text(data.Author).Alignment(TextAlignmentOptions.MidlineRight).Pivot(1).Anchor(AnchorType.MiddleRight)
                   .FontSize(12).Position(-40).FontColor(Color.white.WithAlpha(0.3f))
               - SBgButton.Background(CardButtonBg).Anchor(AnchorType.MiddleRight).Size(150,45).Ppu(3)
                   .Pivot(1,0.5f).Position(-200, 0).Notify(data.OnSettingsClicked).Text("Settings").FontSize(12);
    }

    private static SContainerOptions ModLoaderCard()
    {
        return SContainer.Background(Color.black, EBackground.None).PHeight(120)
               - SLabel.Text($"RedLoader {BuildInfo.Version}").FontSize(25).Font(EFont.FatDebug)
                   .Dock(EDockType.Fill).Alignment(TextAlignmentOptions.MidlineLeft)
                   .FontColor(System.Drawing.Color.Tomato.ToUnityColor()).Margin(50, 0, 0, 0)
               - SLabel.Text("Toni Macaroni").Alignment(TextAlignmentOptions.MidlineRight).Pivot(1).Anchor(AnchorType.MiddleRight)
                   .FontSize(15).Position(-40).FontColor(Color.white.WithAlpha(0.3f));
    }

    private static void OnShowMods()
    {
        TogglePanel(MOD_LIST_ID);
    }

    private class ModCardData
    {
        public string Author;
        public string Name;
        public string Version;

        public string ID;

        public void OnSettingsClicked()
        {
            ModSettingsUi.Open(ID);
        }
    }
}