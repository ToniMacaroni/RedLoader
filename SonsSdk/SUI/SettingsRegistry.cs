using System.Reflection;
using HarmonyLib;
using RedLoader;
using SonsSdk;
using SonsSdk.Attributes;
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
        GetBackgroundSprite(EBackground.Round8), 
        Image.Type.Sliced);

    public static void CreateSettings<T>(ModBase mod, T settingsObject, bool changesNeedRestart = false, Action callback = null)
    {
        CreateSettings(mod, settingsObject, typeof(T));
    }
    
    public static void CreateSettings(
        ModBase mod, 
        object settingsObject,
        Type settingsType,
        bool changesNeedRestart = false, 
        Action callback = null)
    {
        var container = SContainer;
        var configList = new List<SettingsConfigEntry>();
        
        GenerateUi(mod, settingsObject, settingsType, container, configList);

        container.Root.DontDestroyOnLoad().HideAndDontSave().SetActive(false);
        container.Name($"{mod.ID}SettingsPanel");
        
        SettingsEntries[mod.ID] = new SettingsEntry(container, changesNeedRestart, callback, configList);
    }

    private static void GenerateUi(ModBase mod,
        object settingsObject,
        Type settingsType,
        SContainerOptions container,
        List<SettingsConfigEntry> outConfigList)
    {
        SContainerOptions Wrap(SUiElement element, ConfigEntry config, Action customRevertAction = null)
        {
            return SContainer.Horizontal(0, "CE")
                .PHeight(65)
                .Add(element)
                .Add(SContainer.PWidth(100) -
                     SBgButton.Text("Revert")
                         .Background(ButtonBg)
                         .Ppu(3)
                         .Font(EFont.RobotoLight)
                         .Notify(customRevertAction ?? config.ResetToDefault)
                         .Dock(EDockType.Fill)
                         .Margin(7, 0, 7, 7)
                     );
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
                outConfigList.Add(new(entry, optionContainer));
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
                outConfigList.Add(new(entry, optionContainer));
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
                outConfigList.Add(new(entry, optionContainer));
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
                    outConfigList.Add(new(entry, optionContainer));
                }
                else
                {
                    var option = STextbox.Text(entry.DisplayName).Value(observable.Value).Bind(observable).FlexWidth(1);
                    var optionContainer = Wrap(option, entry);
                
                    container.Add(optionContainer);
                    outConfigList.Add(new(entry, optionContainer));
                }
            }
            else if(field.Type == typeof(KeybindConfigEntry))
            {
                var entry = (KeybindConfigEntry) field.GetValue(settingsObject);
                if (entry == null)
                {
                    continue;
                }

                var option = SKeybind.Text(entry.DisplayName.ToUpper()).Config(entry).BindingInputHeight(55);
                var optionContainer = Wrap(option, entry, option.RevertToDefault);
                container.Add(optionContainer);
                outConfigList.Add(new(entry, optionContainer));
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

        var mode = type.GetCustomAttribute<SettingsUiMode>()?.Mode ?? SettingsUiMode.ESettingsUiMode.OptOut;

        foreach (var field in fields)
        {
            if(field.Name.EndsWith("__BackingField"))
                continue;
            
            if (mode == SettingsUiMode.ESettingsUiMode.OptOut && field.GetCustomAttribute<SettingsUiIgnore>() != null)
            {
                RLog.Debug("Ignored field: " + field.Name + " in settings");
                continue;
            }
            
            if (mode == SettingsUiMode.ESettingsUiMode.OptIn && field.GetCustomAttribute<SettingsUiInclude>() == null)
            {
                RLog.Debug("Ignored field: " + field.Name + " in settings (not included)");
                continue;
            }

            RLog.Debug("Added field: " + field.Name + " to settings");
            members.Add(new FieldMemberInfo(field));
        }
        
        foreach (var property in properties)
        {
            if (mode == SettingsUiMode.ESettingsUiMode.OptOut && property.GetCustomAttribute<SettingsUiIgnore>() != null)
            {
                RLog.Debug("Ignored field: " + property.Name + " in settings");
                continue;
            }
            
            if (mode == SettingsUiMode.ESettingsUiMode.OptIn && property.GetCustomAttribute<SettingsUiInclude>() == null)
            {
                RLog.Debug("Ignored field: " + property.Name + " in settings (not included)");
                continue;
            }
            
            RLog.Debug("Added property: " + property.Name + " to settings");
            members.Add(new PropertyMemberInfo(property));
        }
        
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

    public class SettingsConfigEntry
    {
        public readonly ConfigEntry ConfigEntry;
        public readonly SUiElement UiElement;
        private bool _shouldBeVisible = true;

        public bool ShouldBeVisible
        {
            get => _shouldBeVisible;
            set
            {
                _shouldBeVisible = value;
                RefreshVisibility();
            }
        }

        public string CategoryName => ConfigEntry.Category.Identifier;
        
        public SettingsConfigEntry(ConfigEntry configEntry, SUiElement uiElement)
        {
            ConfigEntry = configEntry;
            UiElement = uiElement;
        }
        
        public void RefreshVisibility()
        {
            UiElement.Root.SetActive(_shouldBeVisible);
        }
    }

    public class SettingsEntry
    {
        public SContainerOptions Container;
        public Action Callback;
        public bool ChangesNeedRestart;

        public List<SettingsConfigEntry> ConfigEntries;

        private Dictionary<string, (SLabelDividerOptions, bool)> _dividers;

        public SettingsEntry()
        { }

        public SettingsEntry(SContainerOptions container, bool changesNeedRestart, Action callback, List<SettingsConfigEntry> configEntries)
        {
            Container = container;
            ChangesNeedRestart = changesNeedRestart;
            Callback = callback;
            ConfigEntries = configEntries;
        }

        public bool CheckForChanges()
        {
            foreach (var configEntry in ConfigEntries)
            {
                var cfg = configEntry.ConfigEntry;
                if(cfg.HasChanged)
                {
                    RLog.Debug($"{cfg.DisplayName} has changed ({cfg.GetDefaultValueAsString()} -> {cfg.GetValueAsString()})");
                    return true;
                }
            }
            
            return false;
        }
        
        private void ValidateDividers()
        {
            if (_dividers == null)
            {
                _dividers = new Dictionary<string, (SLabelDividerOptions, bool)>();
            }
            
            if (_dividers.Count == 0)
            {
                foreach (var cfg in ConfigEntries)
                {
                    if(_dividers.ContainsKey(cfg.CategoryName))
                        continue;

                    var divider = SLabelDivider.Text(cfg.CategoryName).FontColor("#ea2f4e40").OnClick(() =>
                    {
                        ToggleCategory(cfg.CategoryName, !_dividers[cfg.CategoryName].Item2);
                    } );
                    _dividers.Add(cfg.CategoryName, (divider, true));
                }
            }
        }

        public void ParentTo(Transform parent)
        {
            ValidateDividers();
            
            var categories = ConfigEntries.GroupBy(x => x.CategoryName).ToDictionary(x => x.Key, x => x.ToList());
            
            foreach (var pair in categories)
            {
                var category = pair.Key;

                _dividers[category].Item1.RectTransform.SetParent(parent, false);
                
                foreach (var cfg in pair.Value)
                {
                    var element = cfg.UiElement;
                    element.RectTransform.SetParent(parent, false);
                    cfg.RefreshVisibility();
                }
            }
        }

        public void Unparent()
        {
            foreach (var entry in ConfigEntries)
            {
                var element = entry.UiElement;
                element.RectTransform.SetParent(Container.RectTransform, false);
                entry.UiElement.Root.SetActive(false);
            }
            
            foreach (var divider in _dividers)
            {
                divider.Value.Item1.RectTransform.SetParent(Container.RectTransform, false);
            }
        }

        public void RevertSettings()
        {
            foreach (var configEntry in ConfigEntries)
            {
                configEntry.ConfigEntry.ResetToDefault();
            }
        }

        public SUiElement GetElementForEntry(ConfigEntry entry)
        {
            return ConfigEntries.Find(x => x.ConfigEntry == entry)?.UiElement;
        }
        
        public void ToggleCategory(string identifier, bool show)
        {
            foreach (var configEntry in ConfigEntries)
            {
                if(configEntry.CategoryName == identifier)
                    configEntry.ShouldBeVisible = show;
            }

            var tuple = _dividers[identifier];
            tuple.Item2 = show;
            _dividers[identifier] = tuple;
        }
    }
}