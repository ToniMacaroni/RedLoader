using SonsSdk;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace SUI;

using static CommonExtensions;

public class SLabelOptions : SUiElement<SLabelOptions, string>
{
    public SLabelOptions(GameObject root) : base(root)
    {
        TextObject = root.GetComponent<TextMeshProUGUI>();
        root.SetActive(true);
    }

    public SLabelOptions Alignment(TextAlignmentOptions alignment)
    {
        TextObject.alignment = alignment;
        return this;
    }

    public SLabelOptions AutoSizeContainer(bool enable = true)
    {
        TextObject.autoSizeTextContainer = enable;
        return this;
    }

    public SLabelOptions Margin(int left, int right, int top, int bottom)
    {
        TextObject.margin = new Vector4(left, top, right, bottom);
        return this;
    }
    
    public SLabelOptions Margin(int margin)
    {
        TextObject.margin = new Vector4(margin, margin, margin, margin);
        return this;
    }
    
    public SLabelOptions Margin(int leftRight, int topBottom)
    {
        TextObject.margin = new Vector4(leftRight, leftRight, topBottom, topBottom);
        return this;
    }
    
    public SLabelOptions Wrap(bool wrap)
    {
        TextObject.enableWordWrapping = wrap;
        TextObject.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        return this;
    }

    public SLabelOptions FontSpacing(float spacing)
    {
        TextObject.characterSpacing = spacing;
        return this;
    }
    
    public SLabelOptions LineSpacing(float spacing)
    {
        TextObject.lineSpacing = spacing;
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