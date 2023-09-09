using System.Reflection;
using HarmonyLib;
using RedLoader;
using SonsSdk;

namespace SUI;

public partial class SUI
{
    public static Dictionary<string, SContainerOptions> SettingsPanels = new();

    public static void CreateSettings(ModBase mod, object settingsObject, Type settingsType)
    {
        var container = SContainer;
        
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

                var option = SSlider.Text(entry.DisplayName).Range(0, 10).Format("0.00").Value(observable.Value).Bind(observable).PHeight(60);
                container.Add(option);
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

                var option = SSlider.Text(entry.DisplayName).Range(0, 10).Value(observable.Value).Bind(observable).PHeight(60);
                container.Add(option);
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

                var option = SToggle.Text(entry.DisplayName).Value(observable.Value).Bind(observable).PHeight(60);
                container.Add(option);
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

                var option = STextbox.Text(entry.DisplayName).Value(observable.Value).Bind(observable).PHeight(60);
                container.Add(option);
            }
        }
        
        SettingsPanels[mod.ID] = container;
    }
    
    public static SContainerOptions GetSettingsPanel(string id)
    {
        if (SettingsPanels.TryGetValue(id, out var container))
        {
            return container;
        }
        
        return null;
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
        
        foreach (var property in properties)
        {
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
}