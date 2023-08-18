using MelonLoader;
using Sons.Gui;
using SonsSdk;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.PropertyVariants;
using UnityEngine.UI;

namespace SUI;

public class SMenuButtonOptions : SUiElement<SMenuButtonOptions>
{
    public Button ButtonObject;

    private TextMeshProUGUI[] _rgbTextObjects = new TextMeshProUGUI[3];

    public SMenuButtonOptions(GameObject root) : base(root)
    {
        ButtonObject = root.GetComponent<Button>();
        TextObject = root.FindGet<TextMeshProUGUI>("ContentPanel/TextBase");
        _rgbTextObjects[0] = root.FindGet<TextMeshProUGUI>("ContentPanel/TextBase/HologramLetteringCanvas/TextRNormal");
        _rgbTextObjects[1] = root.FindGet<TextMeshProUGUI>("ContentPanel/TextBase/HologramLetteringCanvas/TextGNormal");
        _rgbTextObjects[2] = root.FindGet<TextMeshProUGUI>("ContentPanel/TextBase/HologramLetteringCanvas/TextBNormal");
        root.Destroy<LocalizeStringEvent>();
        root.Destroy<GameObjectLocalizer>();
        root.Destroy<SetSelectable>();
        root.Destroy<OnSelectUi>();

        //FontSize(30);
        Text("Button");
        RectTransform.anchorMin = RectTransform.anchorMax = new Vector2(0, 0);
        RectTransform.offsetMin = RectTransform.offsetMax = new Vector2(0, 0);
        TextObject.margin = new Vector4(0, 0, 0, 0);
        MWidth(-1);

        ButtonObject.onClick = new Button.ButtonClickedEvent();
        root.SetActive(true);
    }

    public SMenuButtonOptions Notify(Action action)
    {
        ButtonObject.onClick.AddListener(action);
        return this;
    }

    public override SMenuButtonOptions Text(string text)
    {
        SetRgbText(text);
        return base.Text(text);
    }

    public override SMenuButtonOptions FontSize(int size)
    {
        SetRgbFontSize(size);
        return base.FontSize(size);
    }

    private void SetRgbText(string text)
    {
        _rgbTextObjects[0].text = text;
        _rgbTextObjects[1].text = text;
        _rgbTextObjects[2].text = text;
    }
    
    private void SetRgbFontSize(int size)
    {
        _rgbTextObjects[0].fontSize = size;
        _rgbTextObjects[1].fontSize = size;
        _rgbTextObjects[2].fontSize = size;
    }
}