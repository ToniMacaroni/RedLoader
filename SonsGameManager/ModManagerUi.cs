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
    private static readonly Color ComponentBlack = new(0, 0, 0, 0.6f);

    internal static void CreateUi()
    {
        _ = RegisterNewPanel(MOD_INDICATOR_ID)
                .Pivot(0)
                .Anchor(AnchorType.MiddleLeft)
                .Size(250, 60)
                .Position(10, -300)
                .Background(SpriteBackground400ppu, MainBgBlack, Image.Type.Sliced)
            - SLabel
                .RichText($"Loaded <color=#eb8f34>{SonsMod.RegisteredMelons.Count}</color> {"Mod".MakePlural(SonsMod.RegisteredMelons.Count)}")
                .FontColor(Color.white.WithAlpha(0.3f)).FontSize(18).Dock(EDockType.Fill).Alignment(TextAlignmentOptions.Center);

        var panel = RegisterNewPanel(MOD_LIST_ID)
                        .Dock(EDockType.Fill).RectPadding(100, 400, 250, 400).Background(MainBgBlack, EBackground.Rounded).Vertical(10, "EC").Padding(2)
                    - SLabel.Text("Installed Mods").PHeight(100).FontSize(30).FontSpacing(8.5f)
                    - ModLoaderCard();

        var paddingContainer = SDiv.FlexHeight(1).Id("PaddingContainer");
        paddingContainer.SetParent(panel);
        
        var scroll = SScrollContainer
            .Dock(EDockType.Fill)
            .Background(ComponentBlack, EBackground.Rounded)
            .Size(-20, -20)
            .As<SScrollContainerOptions>();
        scroll.ContainerObject.Spacing(10);
        scroll.SetParent(paddingContainer);
        
        foreach (var mod in SonsMod.RegisteredMelons)
        {
            scroll.Add(ModCard(new ModCardData
            {
                Name = string.IsNullOrEmpty(mod.Manifest.Name) ? mod.ID : mod.Manifest.Name,
                Author = mod.Manifest.Author,
                Version = mod.Manifest.Version
            }));
        }

        AddToTitleMenuButtons(SMenuButton.Text("Mods").MWidth(200).Notify(OnShowMods), 3);

        panel.Active(false);
    }

    private static SContainerOptions ModCard(ModCardData data)
    {
        return SContainer.Background(Color.black.WithAlpha(0.5f), EBackground.None).PHeight(100)
               - SLabel.RichText($"<color=#BBB>{data.Name}</color> <color=#777>{data.Version}</color>").FontSize(22)
                   .Dock(EDockType.Fill).Alignment(TextAlignmentOptions.MidlineLeft).Margin(50, 0, 0, 0)
               - SLabel.Text(data.Author).Alignment(TextAlignmentOptions.MidlineRight).Pivot(1).Anchor(AnchorType.MiddleRight)
                   .FontSize(12).Position(-40).FontColor(Color.white.WithAlpha(0.3f));
    }

    private static SContainerOptions ModLoaderCard()
    {
        return SContainer.Background(Color.black, EBackground.None).PHeight(120)
               - SLabel.Text($"RedLoader {BuildInfo.Version}").FontSize(25)
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
    }
}