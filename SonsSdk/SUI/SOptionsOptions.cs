using System;
using Il2CppSystem.Collections.Generic;
using Sons.UiElements;
using SonsSdk;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.PropertyVariants;
using UnityEngine.UI;

namespace SUI;

public class SOptionsOptions : SUiElement<SOptionsOptions, string>
{
    public SonsDropdown DropdownObject;

    public SOptionsOptions(GameObject root) : base(root)
    {
        DropdownObject = root.FindGet<SonsDropdown>("DropdownPanel/Dropdown");
        TextObject = root.FindGet<TextMeshProUGUI>("LabelPanel/Label");

        root.SetActive(true);
    }

    public SOptionsOptions Options(params string[] options)
    {
        var il2CppList = new Il2CppSystem.Collections.Generic.List<string>();
        foreach (var option in options)
        {
            il2CppList.Add(option);
        }

        DropdownObject.AddOptions(il2CppList);
        return this;
    }

    public SOptionsOptions Value(int value)
    {
        DropdownObject.value = value;
        return this;
    }

    public SOptionsOptions Value(string value)
    {
        var idx = DropdownObject.options.FindIndex(new Func<TMP_Dropdown.OptionData, bool>(x => x.text == value));
        if (idx == -1)
            return this;
        
        DropdownObject.value = idx;
        return this;
    }
    
    public SOptionsOptions HideLabel(bool hide = true)
    {
        TextObject.transform.parent.gameObject.SetActive(!hide);
        return this;
    }
    
    public SOptionsOptions Background(bool hasBackground)
    {
        Root.transform.Find("DropdownPanel/DropdownBacking").gameObject.SetActive(hasBackground);
        return this;
    }
    
    public SOptionsOptions Background(Sprite sprite)
    {
        Root.FindGet<Image>("DropdownPanel/DropdownBacking").sprite = sprite;
        return this;
    }
    
    public SOptionsOptions Background(EBackground background)
    {
        Root.FindGet<Image>("DropdownPanel/DropdownBacking").sprite = SUI.GetBackgroundSprite(background);
        return this;
    }

    public SOptionsOptions LabelWidth(float width)
    {
        var layout = Root.GetComponent<HorizontalLayoutGroup>();
        layout.childForceExpandWidth = false;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.spacing = 0;
        
        var dropDownLayout = Root.FindGet<LayoutElement>("DropdownPanel");
        dropDownLayout.flexibleWidth = 1;

        var labelLayout = TextObject.transform.parent.GetComponent<LayoutElement>();
        labelLayout.minWidth = width;
        
        return this;
    }

    public SOptionsOptions Notify(Action<int> action)
    {
        DropdownObject.onValueChanged.AddListener(action);
        return this;
    }

    public SOptionsOptions Notify(Action<string> action)
    {
        DropdownObject.onValueChanged.AddListener(new Action<int>(x => action(DropdownObject.options._items[x].text)));
        return this;
    }
    
    protected override void RegisterObservable(Observable<string> observable)
    {
        Value(observable.Value);
        DropdownObject.onValueChanged.AddListener((UnityAction<int>)ObservableSetter);
    }
    
    protected override void UnregisterObservable(Observable<string> observable)
    {
        DropdownObject.onValueChanged.RemoveListener((UnityAction<int>)ObservableSetter);
    }
    
    private void ObservableSetter(int value)
    {
        Observable.Value = DropdownObject.options._items[value].text;
    }
    
    protected override void OnObservaleChanged(string value)
    {
        if (DropdownObject.options._items[DropdownObject.value].text == value)
            return;
        
        Value(value);
    }
}