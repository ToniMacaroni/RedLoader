using SonsSdk;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SUI;

public class SColorWheelOptions : SUiElement<SColorWheelOptions, Color>
{
    public ColorPicker ColorWheelControl;
    public Image BackgroundImage;

    public SColorWheelOptions(GameObject root) : base(root)
    {
        ColorWheelControl = root.GetComponentInChildren<ColorPicker>();
        BackgroundImage = root.GetComponent<Image>();

        BackgroundImage.sprite = SUI.GetBackgroundSprite(EBackground.Sons);
    }

    public SColorWheelOptions Value(Color value)
    {
        ColorWheelControl.Color = value;
        return this;
    }

    public SColorWheelOptions BgActive(bool active)
    {
        BackgroundImage.enabled = active;
        return this;
    }
    
    public SColorWheelOptions Background(Color color)
    {
        BackgroundImage.color = color;
        return this;
    }
    
    public SColorWheelOptions Background(Sprite sprite, Image.Type type = Image.Type.Simple)
    {
        BackgroundImage.sprite = sprite;
        BackgroundImage.type = type;
        return this;
    }
    
    public SColorWheelOptions Background(EBackground background, Image.Type type = Image.Type.Simple)
    {
        BackgroundImage.sprite = SUI.GetBackgroundSprite(background);
        BackgroundImage.type = type;
        return this;
    }

    public SColorWheelOptions Notify(Action<Color> action)
    {
        ColorWheelControl.OnColorChanged += action;
        return this;
    }

    protected override void RegisterObservable(Observable<Color> observable)
    {
        Value(observable.Value);
        ColorWheelControl.OnColorChanged += observable.Set;
    }

    protected override void UnregisterObservable(Observable<Color> observable)
    {
        ColorWheelControl.OnColorChanged -= observable.Set;
    }

    protected override void OnObservaleChanged(Color value)
    {
        if (ColorWheelControl.Color == value)
            return;
        
        ColorWheelControl.Color = value;
    }
}