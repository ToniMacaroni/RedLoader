using System.Reflection;
using HarmonyLib;
using RedLoader;
using SonsSdk;
using UnityEngine;
using UnityEngine.UI;
using Color = System.Drawing.Color;

namespace SUI;

using static SUI;

public class SettingsRegistry
{
    public static readonly Dictionary<string, SettingsEntry> SettingsEntries = new();
    
    private static readonly BackgroundDefinition ButtonBg = new(
        ColorFromString("#796C4E"), 
        GetBackgroundSprite(EBackground.Round10), 
        Image.Type.Sliced);

    public static void CreateSettings<T>(ModBase mod, T settingsObject, bool changesNeedRestart = false, Action callback = null)
    {
        CreateSettings(mod, settingsObject, typeof(T));
    }
    
    public static void CreateSettings(ModBase mod, object settingsObject, Type settingsType, bool changesNeedRestart = false, Action callback = null)
    {
        var container = SContainer;
        var configList = new List<ConfigEntry>();
        var uiElements = new List<SUiElement>();
        
        GenerateUi(mod, settingsObject, settingsType, container, configList, uiElements);

        container.Root.DontDestroyOnLoad().HideAndDontSave().SetActive(false);
        container.Name($"{mod.ID}SettingsPanel");
        
        SettingsEntries[mod.ID] = new SettingsEntry(container, changesNeedRestart, callback, configList, uiElements);
    }

    private static void GenerateUi(ModBase mod, object settingsObject, Type settingsType, SContainerOptions container, List<ConfigEntry> outConfigList, List<SUiElement> outUiElements)
    {
        SContainerOptions Wrap(SUiElement element, ConfigEntry config)
        {
            return SContainer.Horizontal(0, "CE").PHeight(60).Add(element).Add(SBgButton.Text("Revert").Background(ButtonBg).Notify(config.ResetToDefault).PWidth(100));
        }
        
        foreach (var field in GetMembers(settingsType))
        {
            if (field.Type == typeof(ConfigEntry<float>))
            {
                var entry = (ConfigEntry<float>) field.GetValue(settingsObject);
                if (entry == null)
                {
                    continue;
                }
                
                var observable = new Observable<float>(entry.Value);
                observable.OnValueChanged += value => entry.Value = value;
                
                entry.OnValueChanged.Subscribe((_,nev) => observable.Value = nev);

                var option = SSlider
                    .Text(entry.DisplayName)
                    .Range(entry.Min ?? 0, entry.Max ?? 10)
                    .Format("0.00").Value(observable.Value).Bind(observable).FlexWidth(1);

                var optionContainer = Wrap(option, entry);
                
                container.Add(optionContainer);
                outConfigList.Add(entry);
                outUiElements.Add(optionContainer);
            }
            else if (field.Type == typeof(ConfigEntry<int>))
            {
                var entry = (ConfigEntry<int>) field.GetValue(settingsObject);
                if (entry == null)
                {
                    continue;
                }
                
                var observable = new Observable<float>(entry.Value);
                observable.OnValueChanged += value => entry.Value = (int)value;
                
                entry.OnValueChanged.Subscribe((_,nev) => observable.Value = nev);

                var option = SSlider
                    .Text(entry.DisplayName)
                    .Range(entry.Min ?? 0, entry.Max ?? 10)
                    .Value(observable.Value).Bind(observable).FlexWidth(1);
                
                var optionContainer = Wrap(option, entry);
                
                container.Add(optionContainer);
                outConfigList.Add(entry);
                outUiElements.Add(optionContainer);
            }
            else if (field.Type == typeof(ConfigEntry<bool>))
            {
                var entry = (ConfigEntry<bool>) field.GetValue(settingsObject);
                if (entry == null)
                {
                    continue;
                }
                
                var observable = new Observable<bool>(entry.Value);
                observable.OnValueChanged += value => entry.Value = value;
                
                entry.OnValueChanged.Subscribe((_,nev) => observable.Value = nev);

                var option = SToggle.Text(entry.DisplayName).Value(observable.Value).Bind(observable).FlexWidth(1);
                
                var optionContainer = Wrap(option, entry);
                
                container.Add(optionContainer);
                outConfigList.Add(entry);
                outUiElements.Add(optionContainer);
            }
            else if (field.Type == typeof(ConfigEntry<string>))
            {
                var entry = (ConfigEntry<string>) field.GetValue(settingsObject);
                if (entry == null)
                {
                    continue;
                }
                
                var observable = new Observable<string>(entry.Value);
                observable.OnValueChanged += value => entry.Value = value;
                
                entry.OnValueChanged.Subscribe((_,nev) => observable.Value = nev);

                if (entry.HasOptions)
                {
                    var option = SOptions.Text(entry.DisplayName).Options(entry.Options.ToArray()).Value(observable.Value).Bind(observable)
                        .FlexWidth(1);
                    var optionContainer = Wrap(option, entry);
                
                    container.Add(optionContainer);
                    outUiElements.Add(optionContainer);
                }
                else
                {
                    var option = STextbox.Text(entry.DisplayName).Value(observable.Value).Bind(observable).FlexWidth(1);
                    var optionContainer = Wrap(option, entry);
                
                    container.Add(optionContainer);
                    outUiElements.Add(optionContainer);
                }

                outConfigList.Add(entry);
            }
        }
    }

    public static bool HasSettings(string id)
    {
        return SettingsEntries.ContainsKey(id);
    }
    
    public static SettingsEntry GetEntry(string id)
    {
        if (SettingsEntries.TryGetValue(id, out var container))
        {
            return container;
        }
        
        return null;
    }
    
    public static bool TryGetEntry(string id, out SettingsEntry entry)
    {
        return SettingsEntries.TryGetValue(id, out entry);
    }

    private static List<MemberInfo> GetMembers(Type type)
    {
        var members = new List<MemberInfo>();
        
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        foreach (var field in fields)
        {
            members.Add(new FieldMemberInfo(field));
        }
        
        // foreach (var property in properties)
        // {
        //     members.Add(new PropertyMemberInfo(property));
        // }
        
        return members;
    }

    private abstract class MemberInfo
    {
        public abstract string Name { get; }
        public abstract Type Type { get; }
        public abstract object GetValue(object obj);
        public abstract void SetValue(object obj, object value);
    }
    
    private class FieldMemberInfo : MemberInfo
    {
        private readonly FieldInfo _fieldInfo;

        public FieldMemberInfo(FieldInfo fieldInfo)
        {
            _fieldInfo = fieldInfo;
        }

        public override string Name => _fieldInfo.Name;
        public override Type Type => _fieldInfo.FieldType;
        public override object GetValue(object obj) => _fieldInfo.GetValue(obj);
        public override void SetValue(object obj, object value) => _fieldInfo.SetValue(obj, value);
    }
    
    private class PropertyMemberInfo : MemberInfo
    {
        private readonly PropertyInfo _propertyInfo;

        public PropertyMemberInfo(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
        }

        public override string Name => _propertyInfo.Name;
        public override Type Type => _propertyInfo.PropertyType;
        public override object GetValue(object obj) => _propertyInfo.GetValue(obj);
        public override void SetValue(object obj, object value) => _propertyInfo.SetValue(obj, value);
    }

    public class SettingsEntry
    {
        public SContainerOptions Container;
        public Action Callback;
        public bool ChangesNeedRestart;
        public List<ConfigEntry> ConfigEntries;
        public List<SUiElement> UiElements = new();

        public SettingsEntry()
        { }

        public SettingsEntry(SContainerOptions container, bool changesNeedRestart, Action callback, List<ConfigEntry> configEntries, List<SUiElement> uiElements)
        {
            Container = container;
            ChangesNeedRestart = changesNeedRestart;
            Callback = callback;
            ConfigEntries = configEntries;
            UiElements = uiElements;
        }

        public bool CheckForChanges()
        {
            foreach (var configEntry in ConfigEntries)
            {
                if(configEntry.HasChanged)
                {
                    RLog.Debug($"{configEntry.DisplayName} has changed ({configEntry.GetDefaultValueAsString()} -> {configEntry.GetValueAsString()})");
                    return true;
                }
            }
            
            return false;
        }

        public void ParentTo(Transform parent)
        {
            foreach (var element in UiElements)
            {
                element.RectTransform.SetParent(parent, false);
            }
        }

        public void Unparent()
        {
            foreach (var element in UiElements)
            {
                element.RectTransform.SetParent(Container.RectTransform, false);
            }
        }

        public void RevertSettings()
        {
            foreach (var configEntry in ConfigEntries)
            {
                configEntry.ResetToDefault();
            }
        }
    }
}