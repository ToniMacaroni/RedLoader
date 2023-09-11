using SonsSdk;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace SUI;

public class SBgButtonOptions : SUiElement<SBgButtonOptions>
{
    public Button ButtonObject;
    public Image ImageObject;

    public SBgButtonOptions(GameObject root) : base(root)
    {
        ButtonObject = root.GetComponent<Button>();
        TextObject = root.FindGet<TextMeshProUGUI>("Text (TMP)");
        ImageObject = root.GetComponent<Image>();
        root.Destroy<LocalizeStringEvent>();

        TextObject.fontSizeMin = 4;
        TextObject.fontSizeMax = 50;
        
        ButtonObject.onClick = new Button.ButtonClickedEvent();
        
        FontSize(20);
        Text("Button");

        ImageObject.color = UnityEngine.Color.white;
        
        root.SetActive(true);
    }

    public SBgButtonOptions Notify(Action action)
    {
        ButtonObject.onClick.AddListener(action);
        return this;
    }
    
    public SBgButtonOptions Notify(Action<SBgButtonOptions> action)
    {
        void OnClickImpl()
        {
            action(this);
        }
        
        ButtonObject.onClick.AddListener((UnityAction)OnClickImpl);
        return this;
    }

    public SBgButtonOptions Color(Color color)
    {
        ButtonObject.colors = new ColorBlock
        {
            colorMultiplier = 1,
            normalColor = color,
            highlightedColor = color.WithBrightnessOffset(0.1f),
            pressedColor = color.WithBrightnessOffset(-0.1f),
            selectedColor = color,
            disabledColor = color
        };
        
        return this;
    }
    
    public SBgButtonOptions Color(Color normal, Color highlight)
    {
        ButtonObject.colors = new ColorBlock
        {
            colorMultiplier = 1,
            normalColor = normal,
            highlightedColor = highlight,
            pressedColor = highlight.WithBrightnessOffset(-0.1f),
            selectedColor = normal,
            disabledColor = normal
        };
        
        return this;
    }
    
    public SBgButtonOptions Color(Color color, float highlightBrightnessOffset)
    {
        ButtonObject.colors = new ColorBlock
        {
            colorMultiplier = 1,
            normalColor = color,
            highlightedColor = color.WithBrightnessOffset(highlightBrightnessOffset),
            pressedColor = color.WithBrightnessOffset(-0.1f),
            selectedColor = color,
            disabledColor = color
        };
        
        return this;
    }
    
    public SBgButtonOptions Color(string colorString)
    {
        Color(SUI.ColorFromString(colorString));

        return this;
    }
    
    public SBgButtonOptions Background(Sprite sprite, Image.Type type = Image.Type.Simple)
    {
        ImageObject.sprite = sprite;
        return this;
    }

    public SBgButtonOptions Background(bool show)
    {
        ImageObject.enabled = show;
        return this;
    }
    
    public SBgButtonOptions Background(EBackground type)
    {
        ImageObject.sprite = SUI.GetBackgroundSprite(type);
        ImageObject.type = type == EBackground.RoundedStandard ? Image.Type.Sliced : Image.Type.Simple;
        return this;
    }
    
    public SBgButtonOptions Background(string color)
    {
        Color(SUI.ColorFromString(color));
        return this;
    }
    
    public SBgButtonOptions Background(SUI.BackgroundDefinition backgroundDefinition)
    {
        ImageObject.sprite = backgroundDefinition.Sprite;
        ImageObject.type = backgroundDefinition.Type;
        Color(backgroundDefinition.Color);
        return this;
    }

    public SBgButtonOptions AutoSize()
    {
        TextObject.autoSizeTextContainer = true;
        
        var sizeFitter = GetOrAdd<ContentSizeFitter>();
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        return this;
    }
}