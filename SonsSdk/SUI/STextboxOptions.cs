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
        InputFieldObject.gameObject.SetActive(true);
        TextObject = root.FindGet<TextMeshProUGUI>("Label");
        TextObject.gameObject.Destroy<LocalizeStringEvent>();
        TextObject.gameObject.SetActive(true);

        var horizontal = root.GetComponent<HorizontalLayoutGroup>();
        horizontal.padding = new RectOffset(0, 0, 0, 0);
        horizontal.spacing = 0;
        horizontal.childForceExpandWidth = true;
        
        PlaceholderObject.color = new Color(1,1,1,0.2f);

        FontAutoSize(false);
        FontSize(20);
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