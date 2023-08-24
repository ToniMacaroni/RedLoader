using System;
using SonsSdk;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace SUI;

public class STextboxOptions : SUiElement<STextboxOptions, string>
{
    public TMP_InputField InputFieldObject;
    public TextMeshProUGUI PlaceholderObject;
    public TMP_Text InputTextObject => InputFieldObject.textComponent;

    public STextboxOptions(GameObject root) : base(root)
    {
        InputFieldObject = root.FindGet<TMP_InputField>("InputPanel/InputField");
        PlaceholderObject = InputFieldObject.placeholder.GetComponent<TextMeshProUGUI>();
        TextObject = root.FindGet<TextMeshProUGUI>("Label");

        root.SetActive(true);
    }

    public STextboxOptions Value(string value)
    {
        InputFieldObject.text = value;
        return this;
    }
    
    public STextboxOptions Placeholder(string value)
    {
        PlaceholderObject.text = value;
        return this;
    }

    public STextboxOptions HideLabel(bool hide = true)
    {
        TextObject.gameObject.SetActive(!hide);
        return this;
    }
    
    public STextboxOptions CuddleLabel(bool enable = true, float spacing = 10)
    {
        var layout = Root.GetComponent<HorizontalLayoutGroup>();
        layout.childForceExpandWidth = !enable;
        layout.spacing = enable ? spacing : 0;
        return this;
    }

    public STextboxOptions InputFlexWidth(float width)
    {
        InputFieldObject.transform.parent.GetComponent<LayoutElement>().flexibleWidth = width;
        return this;
    }
    
    public STextboxOptions Background(Sprite sprite)
    {
        InputFieldObject.image.sprite = sprite;
        return this;
    }
    
    public STextboxOptions Background(EBackground background)
    {
        InputFieldObject.image.sprite = SUI.GetBackgroundSprite(background);
        return this;
    }
    
    public STextboxOptions Notify(Action<string> action)
    {
        InputFieldObject.onValueChanged.AddListener(action);
        return this;
    }

    protected override void RegisterObservable(Observable<string> observable)
    {
        Value(observable.Value);
        InputFieldObject.onValueChanged.AddListener((UnityAction<string>)observable.Set);
    }
    
    protected override void UnregisterObservable(Observable<string> observable)
    {
        InputFieldObject.onValueChanged.RemoveListener((UnityAction<string>)observable.Set);
    }

    protected override void OnObservaleChanged(string value)
    {
        if (InputFieldObject.text == value)
            return;
        
        InputFieldObject.text = value;
    }
}