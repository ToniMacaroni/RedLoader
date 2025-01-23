using System.Collections;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using RedLoader;
using RedLoader.Assertions;
using SonsSdk;
using SonsSdk.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Color = System.Drawing.Color;

namespace SUI;

using static SUI;

public class SettingsRegistry
{
    internal static readonly Dictionary<string, SettingsEntry> SettingsEntries = new();
    
    private static readonly BackgroundDefinition ButtonBg = new(
        ColorFromString("#796C4E"), 
        GetBackgroundSprite(EBackground.Round8), 
        Image.Type.Sliced);

    public static void CreateSettings<T>(ModBase mod, T settingsObject, bool changesNeedRestart = false, Action callback = null)
    {
        CreateSettingsInternal(mod, settingsObject, typeof(T), changesNeedRestart, callback, null);
    }
    
    public static void CreateSettings<T>(ModBase mod, T settingsObject, SContainerOptions userContent, bool changesNeedRestart = false, Action callback = null)
    {
        CreateSettingsInternal(mod, settingsObject, typeof(T), changesNeedRestart, callback, userContent);
    }
    
    public static void CreateSettings(
        ModBase mod, 
        object settingsObject,
        Type settingsType,
        bool changesNeedRestart = false, 
        Action callback = null)
    {
        CreateSettingsInternal(mod, settingsObject, settingsType, changesNeedRestart, callback, null);
    }
    
    public static void CreateSettings(
        ModBase mod, 
        object settingsObject,
        Type settingsType,
        SContainerOptions userContent,
        bool changesNeedRestart = false, 
        Action callback = null)
    {
        CreateSettingsInternal(mod, settingsObject, settingsType, changesNeedRestart, callback, userContent);
    }

    private static void CreateSettingsInternal(
        ModBase mod,
        object settingsObject,
        Type settingsType,
        bool changesNeedRestart,
        Action callback,
        SContainerOptions userContent)
    {
        var container = SContainer;
        var configList = new List<SettingsConfigEntry>();
        
        GenerateUi(mod, settingsObject, settingsType, container, configList);

        container.Root.DontDestroyOnLoad().HideAndDontSave().SetActive(false);
        container.Name($"{mod.ID}SettingsPanel");
        
        var entry = SettingsEntries[mod.ID] = new SettingsEntry(container, changesNeedRestart, callback, configList);
        var classCallback = AccessTools.Method(settingsType, "OnSettingsUiClosed");
        if (classCallback != null)
        {
            entry.ConfigClassCallback = classCallback.CreateDelegate<Action>();
        }

        var settingsEntryField = AccessTools.GetDeclaredFields(settingsType).FirstOrDefault(x=>x.FieldType == typeof(SettingsEntry));
        if (settingsEntryField != null)
        {
            settingsEntryField.SetValue(settingsObject, entry);
        }

        entry.UserContentContainer = userContent;

        if (userContent != null)
        {
            userContent.RectTransform.SetParent(entry.Container.RectTransform, false);
        }
    }

    private static void GenerateUi(ModBase mod,
        object settingsObject,
        Type settingsType,
        SContainerOptions container,
        List<SettingsConfigEntry> outConfigList)
    {
        SContainerOptions Wrap(SUiElement element, ConfigEntry config, float prefHeight, Action customRevertAction = null)
        {
            return SContainer.Horizontal(0, "CE")
                .PHeight(prefHeight)
                .Add(element)
                .Add(SContainer.PWidth(100) -
                     SBgButton.Text("Revert").UpperCase().FontSize(16).FontColor("#c3ba8b")
                         .Background(ButtonBg)
                         .Ppu(3)
                         //.Font(EFont.RobotoLight)
                         .Notify(customRevertAction ?? config.ResetToDefault)
                         .Dock(EDockType.Fill)
                         .Margin(7, 0, 7, 7)
                     );
        }

        var defaultHeight = 65f;
        
        foreach (var field in GetMembers(settingsType))
        {
            var headerAttr = field.Attributes.FirstOrDefault(x => x is SettingsUiHeader) as SettingsUiHeader;
            var spacingAttr = field.Attributes.FirstOrDefault(x => x is SettingsUiSpacing) as SettingsUiSpacing;
            
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

                if (!string.IsNullOrEmpty(entry.Description))
                {
                    option.Tooltip(entry.Description);
                }

                var optionContainer = Wrap(option, entry, defaultHeight);

                container.Add(optionContainer);
                outConfigList.Add(new(entry, optionContainer, option, headerAttr, spacingAttr));
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
                
                if (!string.IsNullOrEmpty(entry.Description))
                {
                    option.Tooltip(entry.Description);
                }
                
                var optionContainer = Wrap(option, entry, defaultHeight);
                
                container.Add(optionContainer);
                outConfigList.Add(new(entry, optionContainer, option, headerAttr, spacingAttr));
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
                
                if (!string.IsNullOrEmpty(entry.Description))
                {
                    option.Tooltip(entry.Description);
                }
                
                var optionContainer = Wrap(option, entry, defaultHeight);
                
                container.Add(optionContainer);
                outConfigList.Add(new(entry, optionContainer, option, headerAttr, spacingAttr));
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
                    var optionContainer = Wrap(option, entry, defaultHeight);
                    
                    if (!string.IsNullOrEmpty(entry.Description))
                    {
                        option.Tooltip(entry.Description);
                    }
                
                    container.Add(optionContainer);
                    outConfigList.Add(new(entry, optionContainer, option, headerAttr, spacingAttr));
                }
                else
                {
                    var option = STextbox.Text(entry.DisplayName).Value(observable.Value).Bind(observable).FlexWidth(1);
                    var optionContainer = Wrap(option, entry, defaultHeight);
                    
                    if (!string.IsNullOrEmpty(entry.Description))
                    {
                        option.Tooltip(entry.Description);
                    }
                
                    container.Add(optionContainer);
                    outConfigList.Add(new(entry, optionContainer, option, headerAttr, spacingAttr));
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
                var optionContainer = Wrap(option, entry, defaultHeight, option.RevertToDefault);
                
                if (!string.IsNullOrEmpty(entry.Description))
                {
                    option.Tooltip(entry.Description);
                }
                
                container.Add(optionContainer);
                outConfigList.Add(new(entry, optionContainer, option, headerAttr, spacingAttr));
            }
            else if(field.Type == typeof(ConfigEntry<UnityEngine.Color>))
            {
                var entry = (ConfigEntry<UnityEngine.Color>) field.GetValue(settingsObject);
                if (entry == null)
                {
                    continue;
                }
                
                var observable = new Observable<UnityEngine.Color>(entry.Value);
                observable.OnValueChanged += value => entry.Value = value;
                
                entry.OnValueChanged.Subscribe((_,nev) => observable.Value = nev);

                // var option = SToggle.Text(entry.DisplayName).Value(observable.Value).Bind(observable).FlexWidth(1);
                var label = SLabel
                    .Text(entry.DisplayName)
                    .FontAutoSize(false)
                    .FontSize(20)
                    .UpperCase()
                    .FontColor(new UnityEngine.Color(0.834f, 0.7804f, 0.7804f))
                    .Alignment(TextAlignmentOptions.MidlineLeft)
                    .Dock(EDockType.Fill);
                var option = SColorWheel.Bind(observable).Pivot(1,0.5f).Anchor(AnchorType.MiddleRight).BgActive(false).Size(200, 200);
                var entryContainer = SContainer.Dock(EDockType.Fill).FlexWidth(1).Add(label).Add(option);
                
                if (!string.IsNullOrEmpty(entry.Description))
                {
                    label.Tooltip(entry.Description);
                }
                
                var optionContainer = Wrap(entryContainer, entry, 200);
                
                container.Add(optionContainer);
                outConfigList.Add(new(entry, optionContainer, option, headerAttr, spacingAttr));
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
        public List<Attribute> Attributes { init; get; }
    }
    
    private class FieldMemberInfo : MemberInfo
    {
        private readonly FieldInfo _fieldInfo;

        public FieldMemberInfo(FieldInfo fieldInfo)
        {
            _fieldInfo = fieldInfo;
            Attributes = fieldInfo.GetCustomAttributes().ToList();
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
            Attributes = propertyInfo.GetCustomAttributes().ToList();
        }

        public override string Name => _propertyInfo.Name;
        public override Type Type => _propertyInfo.PropertyType;
        public override object GetValue(object obj) => _propertyInfo.GetValue(obj);
        public override void SetValue(object obj, object value) => _propertyInfo.SetValue(obj, value);
    }

    public class SettingsConfigEntry
    {
        private static readonly float DEFAULT_HEADER_SHADE = 0.5f;

        public readonly ConfigEntry ConfigEntry;
        public readonly SUiElement UiElement;
        public readonly SUiElement WrappedElement;
        public readonly SettingsUiHeader HeaderInfo;
        public readonly SettingsUiSpacing SpacingInfo;
        private bool _shouldBeVisible = true;

        public List<SUiElement> HeaderElements = new();

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
        
        public SettingsConfigEntry(ConfigEntry configEntry, SUiElement uiElement, SUiElement wrappedElement, SettingsUiHeader headerInfo = null, SettingsUiSpacing spacingInfo = null)
        {
            ConfigEntry = configEntry;
            UiElement = uiElement;
            HeaderInfo = headerInfo;
            SpacingInfo = spacingInfo;
            WrappedElement = wrappedElement;

            CreateSpacing();
            CreateHeader();
        }
        
        public void RefreshVisibility()
        {
            UiElement.Root.SetActive(_shouldBeVisible);
            
            foreach (var element in HeaderElements)
            {
                element.Root.SetActive(_shouldBeVisible);
            }
        }
        
        private void CreateHeader()
        {
            if (HeaderInfo == null)
            {
                return;
            }

            var headerElement = SLabel.Text(HeaderInfo.Text).Alignment(HeaderInfo.Alignment).Wrap(true).PHeight(50);
            if(HeaderInfo.LightFont)
                headerElement.As<SLabelOptions>().Font(EFont.RobotoLight);
            if(HeaderInfo.Color.HasValue)
                headerElement.As<SLabelOptions>().FontColor(HeaderInfo.Color.Value);
            else
                headerElement.As<SLabelOptions>().FontColor("#ea2f4e25");
            headerElement.SetParent(UiElement.RectTransform.parent);
            headerElement.RectTransform.SetSiblingIndex(UiElement.RectTransform.GetSiblingIndex());
            headerElement.Root.SetActive(_shouldBeVisible);
            
            HeaderElements.Add(headerElement);
        }

        private void CreateSpacing()
        {
            if(SpacingInfo == null)
            {
                return;
            }

            var spacingElement = SContainer.PHeight(SpacingInfo.Spacing);
            spacingElement.SetParent(UiElement.RectTransform.parent);
            spacingElement.RectTransform.SetSiblingIndex(UiElement.RectTransform.GetSiblingIndex());
            spacingElement.Root.SetActive(_shouldBeVisible);
            
            HeaderElements.Add(spacingElement);
        }
    }

    public class SettingsEntry
    {
        public SContainerOptions Container;
        public SContainerOptions UserContentContainer;
        internal Action Callback;
        internal Action ConfigClassCallback;
        public bool ChangesNeedRestart;

        public List<SettingsConfigEntry> ConfigEntries;

        private Dictionary<string, (SLabelDividerOptions, bool)> _dividers;

        public SettingsEntry()
        { }

        public SettingsEntry(
            SContainerOptions container,
            bool changesNeedRestart,
            Action callback,
            List<SettingsConfigEntry> configEntries)
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

                    var divider = SLabelDivider.RichText("\uf0d7 " + cfg.CategoryName).FontColor("#ea2f4e40").OnClick(() =>
                    {
                        ToggleCategory(cfg.CategoryName, !_dividers[cfg.CategoryName].Item2);
                    } );
                    _dividers.Add(cfg.CategoryName, (divider, true));
                }
            }
        }
        
        private IEnumerator ColorPickerHack()
        {
            yield return new WaitForSeconds(0.1f);

            foreach (var entry in ConfigEntries)
            {
                if (entry.ConfigEntry is ConfigEntry<UnityEngine.Color> && entry.UiElement.Root.activeSelf)
                {
                    var pickerOptions = entry.WrappedElement.As<SColorWheelOptions>();
                    if (pickerOptions == null)
                    {
                        continue;
                    }
                    
                    pickerOptions.ColorWheelControl.OnEnable();
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
                    foreach (var headerElement in cfg.HeaderElements)
                    {
                        headerElement.RectTransform.SetParent(parent, false);
                    }

                    var element = cfg.UiElement;
                    element.RectTransform.SetParent(parent, false);
                    cfg.RefreshVisibility();
                }
            }
            
            UserContentContainer?.RectTransform.SetParent(parent, false);

            ColorPickerHack().RunCoro();
        }

        public void Unparent()
        {
            UserContentContainer?.RectTransform.SetParent(Container.RectTransform, false);
            
            foreach (var entry in ConfigEntries)
            {
                var element = entry.UiElement;
                element.RectTransform.SetParent(Container.RectTransform, false);
                entry.UiElement.Root.SetActive(false);
                
                foreach (var headerElement in entry.HeaderElements)
                {
                    headerElement.RectTransform.SetParent(Container.RectTransform, false);
                    headerElement.Root.SetActive(false);
                }
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

        /// <summary>
        /// Gets the ui container for a config entry. The container wraps the option element and the revert button.
        /// To get the actual option element use <see cref="GetWrappedElementForEntry"/>.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public SUiElement GetElementForEntry(ConfigEntry entry)
        {
            return ConfigEntries.Find(x => x.ConfigEntry == entry)?.UiElement;
        }
        
        /// <summary>
        /// Get the option element for a config entry. To get the container use <see cref="GetElementForEntry"/>.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public SUiElement GetWrappedElementForEntry(ConfigEntry entry)
        {
            return ConfigEntries.Find(x => x.ConfigEntry == entry)?.WrappedElement;
        }

        /// <summary>
        /// Sets the textboxes character limit. Make sure the config entry is a string entry.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="characterLimit"></param>
        /// <param name="disableAutoSizing">Disable auto sizing of the text inside the textbox</param>
        public void SetCharacterLimit(ConfigEntry entry, int characterLimit, bool disableAutoSizing = true)
        {
            var element = GetElementForEntry(entry);
            if(element == null)
                throw new Exception("Config entry has no element");

            // all entries are wrapped in a container
            var tr = element.RectTransform.GetChild(0);
            if (!tr)
                throw new Exception("Config element has no child");

            var input = tr.Find("InputPanel/InputField").GetComponent<TMP_InputField>();
            if(!input)
                throw new Exception("Config element has no input field");
            
            input.characterLimit = characterLimit;
            if (disableAutoSizing)
            {
                input.textComponent.enableAutoSizing = false;
                input.textComponent.fontSize = 18;
            }
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
            tuple.Item1.RichText((show?"\uf0d7 ":"\uf0da ") + identifier);
            _dividers[identifier] = tuple;
        }

        public void Save()
        {
            foreach (var category in ConfigEntries.GroupBy(x=>x.ConfigEntry.Category).Select(x=>x.Key))
            {
                //RLog.Msg(Color.Orange, $"Category: {category.File.FilePath}");
                category.SaveToFile();
            }
        }

        internal void InvokeCallbacks()
        {
            Callback?.Invoke();
            ConfigClassCallback?.Invoke();
        }
    }
}
