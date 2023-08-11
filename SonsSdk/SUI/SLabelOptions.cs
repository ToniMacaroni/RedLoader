using SonsSdk;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;

namespace SUI;

using static CommonExtensions;

public class SLabelOptions : SUiElement<SLabelOptions, string>
{
    public SLabelOptions(GameObject root) : base(root)
    {
        TextObject = root.GetComponent<TextMeshProUGUI>();
        TextObject.gameObject.Destroy<LocalizeStringEvent>();
        TextObject.margin = new Vector4(0, 0, 0, 0);
        TextObject.enableWordWrapping = false;
        TextObject.fontSizeMin = 0;
        TextObject.fontSizeMax = 60;
        TextObject.enableAutoSizing = false;
        Alignment(TextAlignmentOptions.Center);
    }

    public SLabelOptions Alignment(TextAlignmentOptions alignment)
    {
        TextObject.alignment = alignment;
        return this;
    }

    public SLabelOptions AutoSizeContainer()
    {
        TextObject.autoSizeTextContainer = true;
        return this;
    }

    protected override void RegisterObservable(Observable<string> observable)
    {
        Text(observable.Value);
    }

    protected override void OnObservaleChanged(string value)
    {
        if (TextObject.text == value)
            return;
        
        TextObject.text = value;
    }
}