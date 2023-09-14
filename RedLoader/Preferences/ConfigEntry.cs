using System;
using System.Collections.Generic;
using RedLoader.Preferences;
using Tomlet;
using Tomlet.Exceptions;
using Tomlet.Models;

namespace RedLoader
{
    public abstract class ConfigEntry
    {
        public string Identifier { get; internal set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Comment { get; set; }
        public bool IsHidden { get; set; }
        public bool DontSaveDefault { get; set; }
        public ConfigCategory Category { get; internal set; }
        
        public float? Min { get; protected set; }
        public float? Max { get; protected set; }
        
        public List<string> Options { get; protected set; }
        
        public bool HasOptions => Options != null && Options.Count > 0;

        /// <summary>
        /// Doesn't set the HasChanged flag when set to true
        /// </summary>
        internal bool DontRegisterChanges;

        public virtual bool HasChanged { get; protected set; }

        public abstract object BoxedValue { get; set; }

        public Preferences.ValueValidator Validator { get; internal set; }

        public string GetExceptionMessage(string submsg) 
            => $"Attempted to {submsg} {DisplayName} when it is a {GetReflectedType().FullName}!";

        public abstract Type GetReflectedType();

        public abstract void ResetToDefault();

        public abstract string GetDefaultValueAsString();
        public abstract string GetValueAsString();

        public abstract void Load(TomlValue obj);
        public abstract TomlValue Save();

        public readonly MelonEvent<object, object> OnValueChangedUntyped = new();
        protected void FireUntypedValueChanged(object old, object neew)
        {
            OnValueChangedUntyped.Invoke(old, neew);
        }

        public void SetRange(float min, float max)
        {
            Min = min;
            Max = max;
        }
        
        public void SetOptions(params string[] options)
        {
            Options = new List<string>(options);
        }
        
        public void SetOptions(List<string> options)
        {
            Options = options;
        }
    }

    public class ConfigEntry<T> : ConfigEntry
    {
        private T _backingValue;
        
        public T Value
        {
            get => _backingValue;
            set
            {
                if (Validator != null)
                    value = (T)Validator.EnsureValid(value);

                if ((_backingValue == null && value == null) || (_backingValue != null && _backingValue.Equals(value)))
                    return;

                var old = _backingValue;
                _backingValue = value;
                
                if (!DontRegisterChanges) 
                    HasChanged = true;

                OnValueChanged.Invoke(old, value);
                FireUntypedValueChanged(old, value);
            }
        }

        public T DefaultValue { get; set; }

        public override object BoxedValue
        {
            get => _backingValue;
            set => Value = (T)value;
        }

        public override void ResetToDefault() => Value = DefaultValue;

        /// <summary>
        /// Called when the value is changed. The first parameter is the old value, the second is the new value.
        /// </summary>
        public readonly MelonEvent<T, T> OnValueChanged = new();

        public ConfigEntry(
            string identifier,
            string displayName,
            string description,
            bool isHidden,
            bool dontSaveDefault,
            ConfigCategory category,
            T value,
            ValueValidator validator)
        {
            Identifier = identifier;
            DisplayName = displayName;
            Description = description;
            IsHidden = isHidden;
            DontSaveDefault = dontSaveDefault;
            Category = category;
            DefaultValue = value;
            _backingValue = value;
            Validator = validator;
        }

        public override Type GetReflectedType() => typeof(T);
        public override string GetDefaultValueAsString() => DefaultValue?.ToString();
        public override string GetValueAsString() => Value?.ToString();

        public override void Load(TomlValue obj)
        {
            DontRegisterChanges = true;
            try { Value = TomletMain.To<T>(obj); }
            catch (TomlTypeMismatchException)
            {
                return;
            }
            catch (TomlNoSuchValueException)
            {
                return;
            }
            catch (TomlEnumParseException)
            {
                return;
            }
            DontRegisterChanges = false;
        }
        public override TomlValue Save()
        {
            TomlValue returnval = TomletMain.ValueFrom(Value);

            string valueSpecifier = null;
            if(Min.HasValue || Max.HasValue)
                valueSpecifier = $"Range: {Min?.ToString()??"?"} - {Max?.ToString()??"?"}";
            else if (HasOptions)
                valueSpecifier = $"Options: {string.Join(" | ", Options)}";

            if (valueSpecifier != null)
            {
                returnval.Comments.PrecedingComment = string.IsNullOrEmpty(Description) ?
                    valueSpecifier : $"{Description}\n{valueSpecifier}";
            }
            
            returnval.Comments.InlineComment = Comment;
            if (!string.IsNullOrEmpty(returnval.Comments.InlineComment))
                returnval.Comments.InlineComment.Replace('\n', ' ');
            return returnval;
        }

        internal void SetDefaultValue(T value)
        {
            DontRegisterChanges = false;
            Value = value;
            DontRegisterChanges = true;
        }
    }
}