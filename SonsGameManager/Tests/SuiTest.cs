using RedLoader;
using SonsSdk;
using SUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SysColor = System.Drawing.Color;

namespace SonsGameManager;

using static SUI.SUI;
using static RLog;

public class SuiTest
{
    private static readonly Color TextColor = ColorFromString("#FFFFFF");
    private static readonly Color GrayColor = ColorFromString("#D9D9D9");
    private static readonly Color DarkColor = ColorFromString("#101010");

    private static readonly Color LightGray = ColorFromString("#D9D9D9").WithAlpha(0.1f);

    private static BackgroundDefinition _grayRound28;
    private static BackgroundDefinition _buttonBackground;

    public static void Create()
    {
        _grayRound28 = new(Color.black, GetBackgroundSprite(EBackground.Round28), Image.Type.Sliced);
        _buttonBackground = new(TextColor, GetBackgroundSprite(EBackground.Round10), Image.Type.Sliced);
        
        SContainerOptions vertical;
        var panel = RegisterNewPanel("AssetBrowser", true).Dock(EDockType.Fill).Background(DarkColor, EBackground.None).OverrideSorting(999)
                    - Text("Get ready with <color=#ff0048>SUI</color>.", EFont.RobotoRegular, 100).Position(176, -294)
                    - Text("Welcome to RedLoader", EFont.RobotoLight, 48).Position(176, -870)
                    - SDiv.Position(160, -860).Size(8, 80).Background("#ff0048", true)
                    - (vertical = SDiv.Background(_grayRound28).Size(600, 826).Position(1012, -166).Vertical(10, "EX").Padding(20,20,10,10));

        vertical.Add(SLabelDivider.Text("Sliders").Texture(null).Spacing(40).FontSize(20).FontColor("#ff0048"));
        vertical.Add(SSlider.Text("Change"));
        vertical.Add(SSlider.Text("Change").LabelWidth(110).Range(0,1).Value(0.5f).Format("0.00"));
        vertical.Add(SSlider.Text("Change").Options(SSliderOptions.VisibilityMask.Readout | SSliderOptions.VisibilityMask.Buttons));
        vertical.Add(SSlider.Text("Change").Options(SSliderOptions.VisibilityMask.Buttons));
        vertical.Add(SSlider.Text("Change").Range(0,1).Options(SSliderOptions.VisibilityMask.None).Background(EBackground.RoundNormal));

        vertical.Add(SLabelDivider.Text("Textboxes").Texture(null).Spacing(40).FontSize(20).FontColor("#ff0048"));
        vertical.Add(STextbox.Text("Change"));
        vertical.Add(STextbox.Text("Change").CuddleLabel().Background(EBackground.RoundNormal));
        vertical.Add(STextbox.Text("Change").HideLabel().Background(EBackground.RoundNormal));

        vertical.Add(SLabelDivider.Text("Options").Texture(null).Spacing(40).FontSize(20).FontColor("#ff0048"));
        vertical.Add(SOptions.Text("Change"));
        vertical.Add(SOptions.Text("Change").LabelWidth(110).Background(EBackground.RoundNormal));
        vertical.Add(SOptions.Text("Change").HideLabel().Background(EBackground.RoundNormal));

        var tabPanel = STabController;
        panel.Add(tabPanel.Size(800, 320).Position(150, -455));

        // tabPanel.TabDivider.Background(Color.black, EBackground.None).Size(0, 3);
        tabPanel.HideDivider();
        var tab1 = SContainer.Dock(EDockType.Fill).Background(new Color(0.1f,0.1f,0.1f), EBackground.RoundOutline10);
        var tab2 = SContainer.Dock(EDockType.Fill).Background(new Color(0.1f,0.1f,0.1f), EBackground.RoundOutline10);
        tabPanel.AddTab(new("tab1", "Tab 1", tab1));
        tabPanel.AddTab(new("tab2", "Tab 2", tab2));

        tab1 -= SScrollContainer.RectPadding(10).Id("scroll1")
                - SLabel.FontSize(20).Text("Hello there from tab 1");

        tab2 -= SScrollContainer.RectPadding(10).Id("scroll2")
                - SLabel.FontSize(20).Text("Hello there from tab 2");

        for (int i = 0; i < 10; i++)
        {
            tab1["scroll1"].As<SScrollContainerOptions>().Add(SToggle.Text("PrimToggle " + i).Value(i%3==0).PHeight(50));
        }
        
        for (int i = 0; i < 10; i++)
        {
            tab2["scroll2"].As<SScrollContainerOptions>().Add(SToggle.Text("SecToggle " + i).Value(i%4==0).PHeight(50));
        }
    }

    public static SLabelOptions Text(string text, EFont font, int fontSize)
    {
        return SLabel
            .Anchor(AnchorType.TopLeft)
            .RichText(text)
            .Font(font)
            .FontSize(fontSize)
            .Alignment(TextAlignmentOptions.TopLeft)
            .FontColor(TextColor);
    }
}