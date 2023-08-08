using System;
using Sons.Gui.Options;
using Sons.UiElements;
using SonsSdk;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;

namespace SUI;

public class SSliderOptions : SUiElement<SSliderOptions, float>
{
    public SonsSlider SliderObject;

    public SSliderOptions(GameObject root) : base(root)
    {
        SliderObject = root.GetComponent<LinkSliderToText>()._slider.Cast<SonsSlider>();
        TextObject = root.FindGet<TextMeshProUGUI>("LabelPanel/Label");
        TextObject.gameObject.Destroy<LocalizeStringEvent>();
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

    public SSliderOptions Range(float min, float max)
    {
        return Min(min).Max(max);
    }

    public SSliderOptions IntStep()
    {
        SliderObject.wholeNumbers = true;
        return this;
    }

    public SSliderOptions Value(float value)
    {
        SliderObject.value = value;
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
}