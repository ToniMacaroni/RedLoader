using SonsSdk;
using TMPro;
using UnityEngine;
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

        Color(UnityEngine.Color.black);
        root.SetActive(true);
    }

    public SBgButtonOptions Notify(Action action)
    {
        ButtonObject.onClick.AddListener(action);
        return this;
    }

    public SBgButtonOptions Color(Color color)
    {
        ImageObject.color = color;
        return this;
    }
    
    public SBgButtonOptions Background(Sprite sprite, Image.Type type = Image.Type.Simple)
    {
        ImageObject.sprite = sprite;
        return this;
    }
    
    public SBgButtonOptions Background(EBackground type)
    {
        ImageObject.sprite = SUI.GetBackgroundSprite(type);
        ImageObject.type = type == EBackground.Rounded ? Image.Type.Sliced : Image.Type.Simple;
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