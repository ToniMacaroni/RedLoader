using System;
using Tomlet;
using Tomlet.Exceptions;
using Tomlet.Models;

namespace SFLoader.Preferences
{
    public class ReflectiveConfigCategory
    {
        private readonly Type _systemType;
        private object _value;
        
        internal IO.File File = null;
        
        public string Identifier { get;  internal set; }
        public string DisplayName { get; internal set; }

        internal static ReflectiveConfigCategory Create<T>(string categoryName, string displayName) => new(typeof(T), categoryName, displayName);
        
        private ReflectiveConfigCategory(Type type, string categoryName, string displayName)
        {
            _systemType = type;
            Identifier = categoryName;
            DisplayName = displayName;

            var currentFile = ConfigSystem.DefaultFile;
            if (currentFile.TryGetCategoryTable(Identifier) is not { } table)
                LoadDefaults();
            else
                Load(table);

            ConfigSystem.ReflectiveCategories.Add(this);
        }

        internal void LoadDefaults() => _value = Activator.CreateInstance(_systemType);

        internal void Load(TomlValue tomlValue)
        {
            try { _value = TomletMain.To(_systemType, tomlValue); }
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
        }

        internal TomlValue Save()
        {
            if(_value == null)
                LoadDefaults();
            
            return TomletMain.ValueFrom(_systemType, _value!);
        }

        public T GetValue<T>() where T : new()
        {
            if (typeof(T) != _systemType)
                return default;
            if (_value == null)
                LoadDefaults();
            return (T) _value;
        }
        
        public void SetFilePath(string filepath, bool autoload = true, bool printmsg = true)
        {
            if (File != null)
            {
                var oldfile = File;
                File = null;
                if (!ConfigSystem.IsFileInUse(oldfile))
                {
                    oldfile.FileWatcher.Destroy();
                    ConfigSystem.PrefFiles.Remove(oldfile);
                }
            }
            if (!string.IsNullOrEmpty(filepath) && !ConfigSystem.IsFilePathDefault(filepath))
            {
                File = ConfigSystem.GetPrefFileFromFilePath(filepath);
                if (File == null)
                {
                    File = new IO.File(filepath);
                    ConfigSystem.PrefFiles.Add(File);
                }
            }
            if (autoload)
                ConfigSystem.LoadFileAndRefreshCategories(File, printmsg);
        }

        public void ResetFilePath()
        {
            if (File == null)
                return;
            IO.File oldfile = File;
            File = null;
            if (!ConfigSystem.IsFileInUse(oldfile))
            {
                oldfile.FileWatcher.Destroy();
                ConfigSystem.PrefFiles.Remove(oldfile);
            }
            ConfigSystem.LoadFileAndRefreshCategories(ConfigSystem.DefaultFile);
        }
        
        public void SaveToFile(bool printmsg = true)
        {
            IO.File currentfile = File;
            if (currentfile == null)
                currentfile = ConfigSystem.DefaultFile;

            currentfile.document.PutValue(Identifier, Save());
            try
            {
                currentfile.Save();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error while Saving Preferences to {currentfile.FilePath}: {ex}");
                currentfile.WasError = true;
            }
            if (printmsg)
                MelonLogger.Msg($"MelonPreferences Saved to {currentfile.FilePath}");

            ConfigSystem.OnPreferencesSaved.Invoke(currentfile.FilePath);
        }

        public void LoadFromFile(bool printmsg = true)
        {
            IO.File currentfile = File;
            if (currentfile == null)
                currentfile = ConfigSystem.DefaultFile;
            ConfigSystem.LoadFileAndRefreshCategories(currentfile, printmsg);
        }

        public void DestroyFileWatcher() => File?.FileWatcher.Destroy();
    }
}