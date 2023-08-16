using MelonLoader;
using SonsSdk;
using SUI;
using TMPro;
using UnityEngine;

namespace SonsGameManager;

using static SUI.SUI;

public class ModManagerUi
{
    public const string MOD_INDICATOR_ID = "ModIndicatorPanel";
    public const string MOD_LIST_ID = "ModListPanel";
    
    private static readonly Color MainBgBlack = new Color(0, 0, 0, 0.8f);
    private static readonly Color ComponentBlack = new Color(0, 0, 0, 0.6f);

    internal static void CreateUi()
    {
        _ = RegisterNewPanel(MOD_INDICATOR_ID)
                .Pivot(0)
                .Anchor(AnchorType.MiddleLeft)
                .Size(270, 40)
                .Position(10, -300)
                .Background(MainBgBlack, EBackground.Rounded)
            - SLabel
                .Text($"Loaded {SonsMod.RegisteredMelons.Count} {"Mod".MakePlural(SonsMod.RegisteredMelons.Count)}")
                .FontColor(Color.white.WithAlpha(0.3f)).FontSize(18).Dock(EDockType.Fill).Alignment(TextAlignmentOptions.MidlineLeft)
                .Margin(15,0,0,0)
            - SBgButton
                .Text("Show")
                .Background(EBackground.Rounded).Color(Color.white)
                .Pivot(1).Anchor(AnchorType.MiddleRight).VFill().Size(100, 10)
                .Notify(OnShowMods);

        var panel = RegisterNewPanel(MOD_LIST_ID)
                .Dock(EDockType.Fill).RectPadding(400, 100).Background(MainBgBlack, EBackground.Rounded).Vertical(10, "EC").Padding(2)
            - SLabel.Text("Installed Mods").PHeight(130).FontSize(30).FontSpacing(8.5f)
            - ModLoaderCard();

        foreach (var mod in SonsMod.RegisteredMelons)
        {
            panel.Add(ModCard(new ModCardData
            {
                Name = string.IsNullOrEmpty(mod.Info.Name) ? mod.ID : mod.Info.Name,
                Author = mod.Info.Author,
                Version = mod.Info.Version
            }));
        }
        
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
               - SLabel.Text($"SFLoader {BuildInfo.Version}").FontSize(25)
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
        public string Name;
        public string Author;
        public string Version;
    }
}