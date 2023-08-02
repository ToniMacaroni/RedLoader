using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using Microsoft.Win32;

namespace ModManager;

public class PathTools
{
    public const string SOTF_APP_ID = "1326470";
    
    public static string GetGamePath()
    {
#if DEBUG
                return @"F:\SteamLibrary\steamapps\common\SOTF_Melon\SonsOfTheForest.exe";
#endif
        return GetSteamPath();
    }

    public static string GetSteamPath()
    {
        string steamInstall = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)?.OpenSubKey("SOFTWARE")
            ?.OpenSubKey("WOW6432Node")?.OpenSubKey("Valve")?.OpenSubKey("Steam")?.GetValue("InstallPath")
            ?.ToString();
        if (string.IsNullOrEmpty(steamInstall))
        {
            steamInstall = Registry.LocalMachine.OpenSubKey("SOFTWARE")?.OpenSubKey("WOW6432Node")?.OpenSubKey("Valve")?.OpenSubKey("Steam")
                ?.GetValue("InstallPath")
                ?.ToString();
        }

        if (string.IsNullOrEmpty(steamInstall))
        {
            return null;
        }

        string vdf = Path.Combine(steamInstall, @"steamapps\libraryfolders.vdf");
        if (!File.Exists(vdf))
        {
            return null;
        }

        Regex regex = new Regex("\\s\"(?:\\d|path)\"\\s+\"(.+)\"");
        List<string> steamPaths = new List<string>
        {
            Path.Combine(steamInstall, @"steamapps")
        };

        using (StreamReader reader = new StreamReader(vdf))
        {
            while (reader.ReadLine() is { } line)
            {
                Match match = regex.Match(line);
                if (match.Success)
                {
                    steamPaths.Add(Path.Combine(match.Groups[1].Value.Replace(@"\\", @"\"), @"steamapps"));
                }
            }
        }

        regex = new Regex("\\s\"installdir\"\\s+\"(.+)\"");
        foreach (string path in steamPaths)
        {
            if (File.Exists(Path.Combine(path, @"appmanifest_" + SOTF_APP_ID + ".acf")))
            {
                using StreamReader reader = new StreamReader(Path.Combine(path, @"appmanifest_" + SOTF_APP_ID + ".acf"));
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Match match = regex.Match(line);
                    if (match.Success)
                    {
                        var filePath = Path.Combine(path, @"common", match.Groups[1].Value, "SonsOfTheForest.exe");
                        if (File.Exists(filePath))
                        {
                            return filePath;
                        }
                    }
                }
            }
        }

        return null;
    }
}