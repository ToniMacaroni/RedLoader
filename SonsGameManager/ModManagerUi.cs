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
                .OverrideSorting(0)
            - SLabel
                .Text($"Loaded {SonsMod.RegisteredMelons.Count} {"Mod".MakePlural(SonsMod.RegisteredMelons.Count)}")
                .FontColor(Color.white.WithAlpha(0.3f)).FontSize(18).Dock(EDockType.Fill).Alignment(TextAlignmentOptions.MidlineLeft)
                .Margin(15,0,0,0)
            - SBgButton
                .Text("Show")
                .Background(EBackground.Rounded).Color(Color.white)
                .Pivot(1).Anchor(AnchorType.MiddleRight).VFill().Size(100, 10)
                .Notify(OnShowMods);
    }
    
    private static void OnShowMods()
    {
        
    }

    private static SContainerOptions ModCard()
    {
        return SContainer 
               - SLabel.Text("Mod").Dock(EDockType.Fill);
    }

    private class ModCardData
    {
        
    }
}