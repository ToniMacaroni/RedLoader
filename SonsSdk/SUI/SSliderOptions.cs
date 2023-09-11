using System.Reflection.Emit;
using Endnight.Utilities;
using RedLoader;
using Sons.Gui.Options;
using Sons.UiElements;
using SonsSdk;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace SUI;

public class SSliderOptions : SUiElement<SSliderOptions, float>
{
    public SonsSlider SliderObject;
    
    private LinkSliderToText _linkSliderToText;

    public SSliderOptions(GameObject root) : base(root)
    {
        _linkSliderToText = root.GetComponent<LinkSliderToText>();
        _linkSliderToText.SetRoundToInteger(false);
        SliderObject = _linkSliderToText._slider.Cast<SonsSlider>();
        TextObject = root.FindGet<TextMeshProUGUI>("LabelPanel/Label");
        
        root.SetActive(true);
    }

    public SSliderOptions Min(float min)
    {
        SliderObject.minValue = min;
        return this;
    }

    public SSliderOptions Max(float max)
    {
        SliderObject.maxValue = max;
        return this;
    }

    public SSliderOptions Step(float step)
    {
        SliderObject._stepSize = step;
        return this;
    }

    public SSliderOptions Format(string format)
    {
        _linkSliderToText._formatString = format;
        return this;
    }

    public SSliderOptions Range(float min, float max)
    {
        return Min(min).Max(max);
    }

    public SSliderOptions IntStep()
    {
        SliderObject.wholeNumbers = true;
        _linkSliderToText.SetRoundToInteger(true);
        return this;
    }

    public SSliderOptions Value(float value)
    {
        SliderObject.value = value;
        return this;
    }

    public SSliderOptions Options(VisibilityMask mask)
    {
        var label = mask.HasFlag(VisibilityMask.Label);
        var readout = mask.HasFlag(VisibilityMask.Readout);
        var buttons = mask.HasFlag(VisibilityMask.Buttons);
        
        var t = Root.transform;
        
        TextObject.transform.parent.gameObject.SetActive(label);

        if (readout && buttons)
            return this;
        
        _linkSliderToText._textGroup.SetActive(readout);
        _linkSliderToText.enabled = readout;

        var offsetLeft = 130;
        if(!buttons) offsetLeft -= 45;
        if(!readout) offsetLeft -= 75;
            
        var sliderRect = SliderObject.GetComponent<RectTransform>();
        sliderRect.offsetMin = new Vector2(offsetLeft, 10);
        sliderRect.offsetMax = new Vector2(buttons?-60:-10, -10);

        var leftButton = t.Find("SliderPanel/LeftButton").gameObject;
        leftButton.SetActive(buttons);
        t.Find("SliderPanel/RightButton").gameObject.SetActive(buttons);
        
        if(!readout)
            leftButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

        return this;
    }

    public SSliderOptions Background(bool hasBackground)
    {
        Root.transform.Find("SliderPanel/SliderBacking").gameObject.SetActive(hasBackground);
        return this;
    }
    
    public SSliderOptions Background(Sprite sprite)
    {
        Root.FindGet<Image>("SliderPanel/SliderBacking").sprite = sprite;
        return this;
    }
    
    public SSliderOptions Background(EBackground background)
    {
        Root.FindGet<Image>("SliderPanel/SliderBacking").sprite = SUI.GetBackgroundSprite(background);
        return this;
    }
    
    public SSliderOptions LabelWidth(float width)
    {
        var layout = Root.GetComponent<HorizontalLayoutGroup>();
        layout.childForceExpandWidth = false;

        Root.transform.Find("SliderPanel").gameObject.GetOrAddComponent<LayoutElement>().flexibleWidth = 1;
        TextObject.transform.parent.gameObject.GetOrAddComponent<LayoutElement>().preferredWidth = width;
        
        return this;
    }
    
    public SSliderOptions InputFlexWidth(float width)
    {
        var layout = Root.GetComponent<HorizontalLayoutGroup>();
        layout.childForceExpandWidth = false;

        Root.transform.Find("SliderPanel").gameObject.GetOrAddComponent<LayoutElement>().flexibleWidth = width;
        TextObject.transform.parent.gameObject.GetOrAddComponent<LayoutElement>().flexibleWidth = 1;
        
        return this;
    }

    public SSliderOptions Notify(Action<float> action)
    {
        SliderObject.onValueChanged.AddListener(action);
        return this;
    }

    protected override void RegisterObservable(Observable<float> observable)
    {
        SliderObject.onValueChanged.AddListener((UnityAction<float>)observable.Set);
    }

    protected override void UnregisterObservable(Observable<float> observable)
    {
        SliderObject.onValueChanged.RemoveListener((UnityAction<float>)observable.Set);
    }

    protected override void OnObservaleChanged(float value)
    {
        if (Math.Abs(SliderObject.value - value) < 1e-4)
            return;
        
        SliderObject.value = value;
    }

    [Flags]
    public enum VisibilityMask
    {
        None = 0,
        Label = 1 << 0,
        Readout = 1 << 1,
        Buttons = 1 << 2,
    }
}