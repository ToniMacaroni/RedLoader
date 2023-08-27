using System.Reflection;
using MonoMod.Utils;
using RedLoader;
using SonsSdk.Attributes;
using UnityEngine;

namespace SonsSdk;

public class MemberDebugger
{
    private static Dictionary<object, MemberDebugger> _debuggers = new();
    
    public static void StartDebugging(object obj)
    {
        if (_debuggers.TryGetValue(obj, out var debugger))
        {
            debugger.Start();
            return;
        }
        
        _debuggers.Add(obj, new(obj));
    }
    
    public static void StartDebugging(object obj, params string[] propNames)
    {
        if (_debuggers.TryGetValue(obj, out var debugger))
        {
            debugger.Start();
            return;
        }
        
        _debuggers.Add(obj, new(obj, propNames));
    }

    public static void StopDebugging(object obj)
    {
        if (!_debuggers.TryGetValue(obj, out var debugger))
        {
            return;
        }

        debugger.Stop();
    }
    
    private void DrawDebugGui()
    {
        GUI.matrix = Matrix4x4.Scale(Vector3.one * DebugGuiSize);
        GUILayout.BeginArea(new Rect(20, 20, 300, 500));
        GUILayout.BeginVertical(GUI.skin.box);
        
        foreach (var debuggingComponent in _debuggingComponents)
        {
            DrawDebuggingComponent(debuggingComponent);
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void DrawDebuggingComponent(DebuggingComponent comp)
    {
        GUILayout.BeginHorizontal();
        
        GUILayout.Label(comp.Name, GUILayout.Width(200));
        var result = comp.Getter.Invoke(_objectInstance);
        if(result != null)
            GUILayout.Label(result.ToString());

        GUILayout.EndHorizontal();
    }

    public MemberDebugger(object obj)
    {
        _objectInstance = obj;
        
        var props = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var prop in props)
        {
            var attr = prop.GetCustomAttribute<DebugPropAttribute>();
            if (attr != null)
            {
                _debuggingComponents.Add(new(prop));
                RLog.Msg($"Registered debug prop '{prop.Name}'");
            }
        }
        
        Start();
    }
    
    public MemberDebugger(object obj, params string[] propNames)
    {
        _objectInstance = obj;
        
        var props = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var prop in props)
        {
            if (propNames.Contains(prop.Name))
            {
                _debuggingComponents.Add(new(prop));
                RLog.Msg($"Registered debug prop '{prop.Name}'");
            }
        }
        
        Start();
    }

    private void Start()
    {
        if(_isDebugging)
            return;
        
        GlobalEvents.OnGUI.Subscribe(DrawDebugGui);
        _isDebugging = true;
    }
    
    private void Stop()
    {
        if(!_isDebugging)
            return;
        
        _isDebugging = false;
        GlobalEvents.OnGUI.Unsubscribe(DrawDebugGui);
    }

    private object _objectInstance;
    private bool _isDebugging;
    private const float DebugGuiSize = 1.5f;
    private List<DebuggingComponent> _debuggingComponents = new();

    private struct DebuggingComponent
    {
        public Type Type;
        public string Name;
        public FastReflectionDelegate Getter;
        
        public DebuggingComponent(PropertyInfo prop)
        {
            Type = prop.PropertyType;
            Name = prop.Name;
            Getter = prop.GetGetMethod().GetFastDelegate();
        }
    }
}