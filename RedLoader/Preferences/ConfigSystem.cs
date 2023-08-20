using System;
using System.Collections.Generic;
using System.IO;
using RedLoader.Preferences;
using RedLoader.Utils;
using Tomlet.Exceptions;

namespace RedLoader
{
    public static class ConfigSystem
    {
        public static readonly List<ConfigCategory> Categories = new List<ConfigCategory>();
        public static readonly List<ReflectiveConfigCategory> ReflectiveCategories = new List<ReflectiveConfigCategory>();
        public static readonly TomlMapper Mapper = new TomlMapper();

        /// <summary>
        /// Occurs when a Preferences File has been loaded.
        /// <para>
        /// <see cref="string"/>: Path of the Preferences File.
        /// </para>
        /// </summary>
        public static readonly MelonEvent<string> OnPreferencesLoaded = new MelonEvent<string>();

        /// <summary>
        /// Occurs when a Preferences File has been saved.
        /// <para>
        /// <see cref="string"/>: Path of the Preferences File.
        /// </para>
        /// </summary>
        public static readonly MelonEvent<string> OnPreferencesSaved = new MelonEvent<string>();

        internal static List<Preferences.IO.File> PrefFiles = new List<Preferences.IO.File>();
        internal static Preferences.IO.File DefaultFile = null;

        static ConfigSystem() => DefaultFile = new Preferences.IO.File(
            Path.Combine(MelonEnvironment.UserDataDirectory, "_RedLoader.cfg"),
            Path.Combine(MelonEnvironment.UserDataDirectory, "modprefs.ini"));
        
        /// <summary>
        /// Gets the absulute path of a Preferences File.
        /// </summary>
        /// <param name="fileName">Filename relative to the userdata path</param>
        /// <returns>The absolute filepath</returns>
        public static string GetFilePath(string fileName) => Path.Combine(MelonEnvironment.UserDataDirectory, fileName);
        
        /// <summary>
        /// Create a new category which will be saved to a custom file.
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="display_name"></param>
        /// <param name="file_name">The relative filepath</param>
        /// <returns></returns>
        public static ConfigCategory CreateFileCategory(string identifier, string display_name, string file_name)
        {
            var cat = new ConfigCategory(identifier, display_name);
            cat.SetFilePath(GetFilePath(file_name));
            return cat;
        }

        public static void Load()
        {
            try
            {
                DefaultFile.LegacyLoad();
            }
            catch (Exception ex)
            {
                RLog.Error($"Error while Loading Legacy Preferences from {DefaultFile.LegacyFilePath}: {ex}");
                DefaultFile.WasError = true;
            }

            try
            {
                DefaultFile.Load();
            }
            catch (Exception ex)
            {
                RLog.Error($"Error while Loading Preferences from {DefaultFile.FilePath}: {ex}");
                DefaultFile.WasError = true;
            }

            if (PrefFiles.Count >= 0)
            {
                foreach (Preferences.IO.File file in PrefFiles)
                {
                    try
                    {
                        file.Load();
                    }
                    catch (Exception ex)
                    {
                        RLog.Error($"Error while Loading Preferences from {file.FilePath}: {ex}");
                        file.WasError = true;
                    }
                }
            }

            if (Categories.Count > 0)
            {
                foreach (ConfigCategory category in Categories)
                {
                    if (category.Entries.Count <= 0)
                        continue;
                    Preferences.IO.File currentFile = category.File;
                    if (currentFile == null)
                        currentFile = DefaultFile;
                    if (currentFile.WasError)
                        continue;
                    foreach (ConfigEntry entry in category.Entries)
                        currentFile.SetupEntryFromRawValue(entry);
                }
            }

            if (ReflectiveCategories.Count > 0)
            {
                foreach (ReflectiveConfigCategory category in ReflectiveCategories)
                {
                    Preferences.IO.File currentFile = category.File;
                    if (currentFile == null)
                        currentFile = DefaultFile;
                    if (currentFile.WasError)
                        continue;
                    if (!(currentFile.TryGetCategoryTable(category.Identifier) is { } table))
                    {
                        category.LoadDefaults();
                        continue;
                    }

                    category.Load(table);

                    OnPreferencesLoaded.Invoke(currentFile.FilePath);
                }
            }

            RLog.Msg("Preferences Loaded!");
        }

        public static void Save()
        {
            if (Categories.Count > 0)
            {
                foreach (ConfigCategory category in Categories)
                {
                    Preferences.IO.File currentFile = category.File;
                    if (currentFile == null)
                        currentFile = DefaultFile;
                    foreach (ConfigEntry entry in category.Entries)
                        if (!(entry.DontSaveDefault && entry.GetValueAsString() == entry.GetDefaultValueAsString()) && entry.GetValueAsString() != null)
                            currentFile.InsertIntoDocument(category.Identifier, entry.Identifier, entry.Save());
                }
            }
            if (ReflectiveCategories.Count > 0)
            {
                foreach (ReflectiveConfigCategory category in ReflectiveCategories)
                {
                    Preferences.IO.File currentFile = category.File;
                    if (currentFile == null)
                        currentFile = DefaultFile;
                    currentFile.document.PutValue(category.Identifier, category.Save());
                }
            }

            try
            {
                DefaultFile.Save();
            }
            catch (Exception ex)
            {
                RLog.Error($"Error while Saving Preferences to {DefaultFile.FilePath}: {ex}");
                DefaultFile.WasError = true;
            }

            if (PrefFiles.Count >= 0)
            {
                foreach (Preferences.IO.File file in PrefFiles)
                {
                    try
                    {
                        file.Save();
                    }
                    catch (Exception ex)
                    {
                        RLog.Error($"Error while Saving Preferences to {file.FilePath}: {ex}");
                        file.WasError = true;
                        continue;
                    }
                    OnPreferencesSaved.Invoke(file.FilePath);
                }
            }

            RLog.Msg("Preferences Saved!");
        }

        public static ConfigCategory CreateCategory(string identifier) => CreateCategory(identifier, null, false);
        public static ConfigCategory CreateCategory(string identifier, string display_name = null) => CreateCategory(identifier, display_name, false);

        public static ConfigCategory CreateCategory(string identifier, string display_name = null, bool is_hidden = false, bool should_save = true)
        {
            if (string.IsNullOrEmpty(identifier))
                throw new Exception("identifier is null or empty when calling CreateCategory");
            if (display_name == null)
                display_name = identifier;
            ConfigCategory category = GetCategory(identifier);
            if (category != null)
                return category;
            return new ConfigCategory(identifier, display_name, is_hidden);
        }

        public static ReflectiveConfigCategory CreateCategory<T>(string identifier, string display_name = null) where T : new() => ReflectiveConfigCategory.Create<T>(identifier, display_name);

        [Obsolete]
        public static ConfigEntry CreateEntry<T>(string category_identifier, string entry_identifier,
            T default_value, string display_name, bool is_hidden)
            => CreateEntry(category_identifier, entry_identifier, default_value, display_name, null, is_hidden, false, null);

        public static ConfigEntry<T> CreateEntry<T>(string category_identifier, string entry_identifier, T default_value,
            string display_name = null, string description = null, bool is_hidden = false, bool dont_save_default = false,
            ValueValidator validator = null)
        {
            if (string.IsNullOrEmpty(category_identifier))
                throw new Exception("category_identifier is null or empty when calling CreateEntry");

            if (string.IsNullOrEmpty(entry_identifier))
                throw new Exception("entry_identifier is null or empty when calling CreateEntry");

            ConfigCategory category = GetCategory(entry_identifier);
            if (category == null)
                category = CreateCategory(category_identifier);

            return category.CreateEntry(entry_identifier, default_value, display_name, description, is_hidden, dont_save_default, validator);
        }

        public static ConfigCategory GetCategory(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                throw new Exception("identifier is null or empty when calling GetCategory");
            if (Categories.Count <= 0)
                return null;
            return Categories.Find(x => x.Identifier.Equals(identifier));
        }

        public static T GetCategory<T>(string identifier) where T : new()
        {
            if (string.IsNullOrEmpty(identifier))
                throw new Exception("identifier is null or empty when calling GetCategory");
            if (ReflectiveCategories.Count <= 0)
                return default;
            ReflectiveConfigCategory category = ReflectiveCategories.Find(x => x.Identifier.Equals(identifier));
            if (category != null)
                return category.GetValue<T>();
            return default;
        }

        public static void SaveCategory<T>(string identifier, bool printmsg = true)
        {
            if (string.IsNullOrEmpty(identifier))
                throw new Exception("identifier is null or empty when calling GetCategory");
            if (ReflectiveCategories.Count <= 0)
                return;
            ReflectiveConfigCategory category = ReflectiveCategories.Find(x => x.Identifier.Equals(identifier));
            if (category != null)
                category.SaveToFile(printmsg);
        }

        public static ConfigEntry GetEntry(string category_identifier, string entry_identifier) => GetCategory(category_identifier)?.GetEntry(entry_identifier);
        public static ConfigEntry<T> GetEntry<T>(string category_identifier, string entry_identifier) => GetCategory(category_identifier)?.GetEntry<T>(entry_identifier);
        public static bool HasEntry(string category_identifier, string entry_identifier) => (GetEntry(category_identifier, entry_identifier) != null);

        public static void SetEntryValue<T>(string category_identifier, string entry_identifier, T value)
        {
            var entry = GetCategory(category_identifier)?.GetEntry<T>(entry_identifier);
            if (entry != null) entry.Value = value;
        }

        public static T GetEntryValue<T>(string category_identifier, string entry_identifier)
        {
            ConfigCategory cat = GetCategory(category_identifier);
            if (cat == null)
                return default;
            var entry = cat.GetEntry<T>(entry_identifier);
            if (entry == null)
                return default;
            return entry.Value;
        }

        internal static Preferences.IO.File GetPrefFileFromFilePath(string filepath)
        {
            if (PrefFiles.Count <= 0)
                return null;
            FileInfo filepathinfo = new FileInfo(filepath);
            foreach (Preferences.IO.File file in PrefFiles)
            {
                FileInfo filepathinfo2 = new FileInfo(file.FilePath);
                if (filepathinfo.FullName.Equals(filepathinfo2.FullName))
                    return file;
            }

            return null;
        }

        internal static bool IsFileInUse(Preferences.IO.File file)
        {
            if (Categories.Count <= 0)
                return false;
            if (file == null)
                file = DefaultFile;
            if (file == DefaultFile)
                return true;
            foreach (ConfigCategory category in Categories)
            {
                Preferences.IO.File currentFile = category.File;
                if (currentFile == null)
                    currentFile = DefaultFile;
                if (currentFile == file)
                    return true;
            }

            foreach (ReflectiveConfigCategory category in ReflectiveCategories)
            {
                Preferences.IO.File currentFile = category.File;
                if (currentFile == null)
                    currentFile = DefaultFile;
                if (currentFile == file)
                    return true;
            }

            return false;
        }

        internal static void LoadFileAndRefreshCategories(Preferences.IO.File file, bool printmsg = true)
        {
            try
            {
                file.Load();
            }
            catch (TomlUnescapedUnicodeControlCharException ex)
            {
                RLog.Error($"Error while Loading Preferences from {file.FilePath}: {ex}");
                return;
            }
            catch (Exception ex)
            {
                RLog.Error($"Error while Loading Preferences from {file.FilePath}: {ex}");
                file.WasError = true;
                return;
            }

            if (Categories.Count > 0)
                foreach (ConfigCategory category in Categories)
                {
                    Preferences.IO.File currentFile = category.File;
                    if (currentFile == null)
                        currentFile = DefaultFile;
                    if ((currentFile != file) || (category.Entries.Count <= 0))
                        continue;
                    foreach (ConfigEntry entry in category.Entries)
                        currentFile.SetupEntryFromRawValue(entry);
                }

            if (ReflectiveCategories.Count > 0)
                foreach (ReflectiveConfigCategory category in ReflectiveCategories)
                {
                    Preferences.IO.File currentFile = category.File;
                    if (currentFile == null)
                        currentFile = DefaultFile;
                    if (currentFile != file)
                        continue;
                    if (!(file.TryGetCategoryTable(category.Identifier) is { } table))
                    {
                        category.LoadDefaults();
                        continue;
                    }
                    category.Load(table);
                }

            if (printmsg)
                RLog.MsgDirect($"MelonPreferences Loaded from {file.FilePath}");

            OnPreferencesLoaded.Invoke(file.FilePath);
        }

        public static void RemoveCategoryFromFile(string filePath, string categoryName)
        {
            Preferences.IO.File currentFile = GetPrefFileFromFilePath(filePath);
            if (currentFile == null)
                return;
            currentFile.RemoveCategoryFromDocument(categoryName);
        }

        internal static bool IsFilePathDefault(string filepath) => new FileInfo(filepath).FullName.Equals(new FileInfo(DefaultFile.FilePath).FullName);
    }
}