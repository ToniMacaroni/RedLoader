using SonsSdk;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SUI;

public class SToggleOptions : SUiElement<SToggleOptions, bool>
{
    public Toggle ToggleObject;

    public SToggleOptions(GameObject root) : base(root)
    {
        ToggleObject = root.GetComponent<Toggle>();
        TextObject = root.FindGet<TextMeshProUGUI>("Label");
    }
    
    public SToggleOptions Value(bool value)
    {
        ToggleObject.isOn = value;
        return this;
    }

    public SToggleOptions Notify(Action<bool> action)
    {
        ToggleObject.onValueChanged.AddListener(action);
        return this;
    }

    protected override void RegisterObservable(Observable<bool> observable)
    {
        Value(observable.Value);
        ToggleObject.onValueChanged.AddListener((UnityAction<bool>)observable.Set);
    }
    
    protected override void UnregisterObservable(Observable<bool> observable)
    {
        ToggleObject.onValueChanged.RemoveListener((UnityAction<bool>)observable.Set);
    }

    protected override void OnObservaleChanged(bool value)
    {
        if (ToggleObject.isOn == value)
            return;
        
        ToggleObject.isOn = value;
    }
}