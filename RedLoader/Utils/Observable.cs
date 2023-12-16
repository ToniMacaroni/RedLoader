using System;
using System.Drawing;
using RedLoader;

namespace SUI;

public enum EObservableMode
{
    Read,
    Write,
    ReadWrite
}

public class Observable
{
    public static EObservableMode GetModeFromString(string mode)
    {
        switch (mode)
        {
            case "r":
                return EObservableMode.Read;
            case "w":
                return EObservableMode.Write;
            case "rw":
                return EObservableMode.ReadWrite;
            default:
                throw new Exception($"Unknown mode: {mode}");
        }
    }
}

public class Observable<T> : Observable
{
    public event Action<T> OnValueChanged;
    
    private T _value;

    public T Value
    {
        get => _value;
        set
        {
            if ((_value == null && value == null) || (_value != null && _value.Equals(value)))
            {
                return;
            }

            _value = value;
            OnValueChanged?.Invoke(_value);
        }
    }

    public Observable(T value)
    {
        _value = value;
    }
    
    public void SetNoNotify(T value)
    {
        _value = value;
    }
    
    public void Set(T value)
    {
        Value = value;
    }

    /// <summary>
    /// Used to trigger all events.
    /// </summary>
    public void CallValueChanged()
    {
        OnValueChanged?.Invoke(_value);
    }
    
    public void RegisterAndCall(Action<T> action)
    {
        OnValueChanged += action;
        action.Invoke(_value);
    }

    /// <summary>
    /// Removes all event listeners.
    /// </summary>
    public void RemoveEvents()
    {
        OnValueChanged = null;
    }
}

public static class ObservableExtensions
{
    /// <summary>
    /// Creates a new observable with the config value.
    /// Also registers <see cref="Observable{T}.OnValueChanged"/> so that the config entry is updated when the observalble value changes.
    /// </summary>
    /// <param name="configEntry"></param>
    /// <param name="twoWay">If true updates the observable when the config entry changes</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Observable<T> ToObservable<T>(this ConfigEntry<T> configEntry, bool twoWay = false)
    {
        var observable = new Observable<T>(configEntry.Value);
        observable.OnValueChanged += configEntry.SetDefaultValue;
        if (twoWay)
        {
            configEntry.OnValueChanged.Subscribe((old, neww) => observable.Set(neww));
        }
        return observable;
    }
}