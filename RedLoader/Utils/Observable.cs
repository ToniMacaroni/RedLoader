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
            if (_value.Equals(value))
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
}

public static class ObservableExtensions
{
    public static Observable<T> ToObservable<T>(this ConfigEntry<T> configEntry)
    {
        var observable = new Observable<T>(configEntry.Value);
        observable.OnValueChanged += configEntry.SetDefaultValue;
        return observable;
    }
}