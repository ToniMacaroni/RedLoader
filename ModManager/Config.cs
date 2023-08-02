using System;
using System.IO;
using MelonLoader.Tomlyn.Model;
using MelonLoader.Tomlyn.Syntax;
using ModManager.Libs.Tomlyn;

namespace ModManager;

internal static class Config
{
    internal static string RepoApiInstaller = "https://api.github.com/repos/LavaGang/MelonLoader.Installer/releases";
    internal static string RepoApiMelonLoader = "https://api.github.com/repos/LavaGang/MelonLoader/releases";
    internal static string DownloadMelonLoader = "https://github.com/LavaGang/MelonLoader/releases/download";

    internal static string LinkDiscord = "https://discord.gg/2Wn3N2P";
    internal static string LinkTwitter = "https://twitter.com/lava_gang";
    internal static string LinkGitHub = "https://github.com/LavaGang";
    internal static string LinkWiki = "https://melonwiki.xyz";
    internal static string LinkUpdate = "https://github.com/LavaGang/MelonLoader.Installer/releases/latest";

    private static readonly string FilePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MelonLoader.Installer.cfg");

    private static int _theme;

    private static bool _autoupdateinstaller = true;

    private static bool _closeaftercompletion = true;

    private static bool _showalphaprereleases;

    private static bool _rememberlastselectedgame;

    private static string _lastselectedgamepath;

    private static bool _highlightlogfilelocation = true;

    internal static int Theme
    {
        get => _theme;
        set
        {
            _theme = value;
            Save();
        }
    }

    internal static bool AutoUpdateInstaller
    {
        get => _autoupdateinstaller;
        set
        {
            _autoupdateinstaller = value;
            Save();
        }
    }

    internal static bool CloseAfterCompletion
    {
        get => _closeaftercompletion;
        set
        {
            _closeaftercompletion = value;
            Save();
        }
    }

    internal static bool ShowAlphaPreReleases
    {
        get => _showalphaprereleases;
        set
        {
            _showalphaprereleases = value;
            Save();
        }
    }

    internal static bool RememberLastSelectedGame
    {
        get => _rememberlastselectedgame;
        set
        {
            _rememberlastselectedgame = value;
            Save();
        }
    }

    internal static string LastSelectedGamePath
    {
        get => _lastselectedgamepath;
        set
        {
            _lastselectedgamepath = value;
            Save();
        }
    }

    internal static bool HighlightLogFileLocation
    {
        get => _highlightlogfilelocation;
        set
        {
            _highlightlogfilelocation = value;
            Save();
        }
    }

    internal static void Load()
    {
        if (!File.Exists(FilePath))
        {
            return;
        }

        var filestr = File.ReadAllText(FilePath);
        if (string.IsNullOrEmpty(filestr))
        {
            return;
        }

        var doc = Toml.Parse(filestr);
        if (doc == null || doc.HasErrors)
        {
            return;
        }

        var tbl = doc.ToModel();
        if (tbl.Count <= 0 || !tbl.ContainsKey("Installer"))
        {
            return;
        }

        var installertbl = (TomlTable)tbl["Installer"];
        if (installertbl == null || installertbl.Count <= 0)
        {
            return;
        }

        if (installertbl.ContainsKey("Theme"))
        {
            int.TryParse(installertbl["Theme"].ToString(), out _theme);
        }

        if (installertbl.ContainsKey("AutoUpdateInstaller"))
        {
            bool.TryParse(installertbl["AutoUpdateInstaller"].ToString(), out _autoupdateinstaller);
        }

        if (installertbl.ContainsKey("CloseAfterCompletion"))
        {
            bool.TryParse(installertbl["CloseAfterCompletion"].ToString(), out _closeaftercompletion);
        }

        if (installertbl.ContainsKey("ShowAlphaPreReleases"))
        {
            bool.TryParse(installertbl["ShowAlphaPreReleases"].ToString(), out _showalphaprereleases);
        }

        if (installertbl.ContainsKey("RememberLastSelectedGame"))
        {
            bool.TryParse(installertbl["RememberLastSelectedGame"].ToString(), out _rememberlastselectedgame);
        }

        if (installertbl.ContainsKey("LastSelectedGamePath"))
        {
            _lastselectedgamepath = installertbl["LastSelectedGamePath"].ToString();
        }

        if (installertbl.ContainsKey("HighlightLogFileLocation"))
        {
            bool.TryParse(installertbl["HighlightLogFileLocation"].ToString(), out _highlightlogfilelocation);
        }
    }

    internal static void Save()
    {
        var doc = new DocumentSyntax();
        var tbl = new TableSyntax("Installer");
        tbl.Items.Add(new KeyValueSyntax("Theme", new IntegerValueSyntax(_theme)));
        tbl.Items.Add(new KeyValueSyntax("AutoUpdateInstaller", new BooleanValueSyntax(_autoupdateinstaller)));
        tbl.Items.Add(new KeyValueSyntax("CloseAfterCompletion", new BooleanValueSyntax(_closeaftercompletion)));
        tbl.Items.Add(new KeyValueSyntax("ShowAlphaPreReleases", new BooleanValueSyntax(_showalphaprereleases)));
        tbl.Items.Add(new KeyValueSyntax("RememberLastSelectedGame", new BooleanValueSyntax(_rememberlastselectedgame)));
        tbl.Items.Add(new KeyValueSyntax("LastSelectedGamePath",
            new StringValueSyntax(string.IsNullOrEmpty(_lastselectedgamepath) ? "" : _lastselectedgamepath)));
        tbl.Items.Add(new KeyValueSyntax("HighlightLogFileLocation", new BooleanValueSyntax(_highlightlogfilelocation)));
        doc.Tables.Add(tbl);
        File.WriteAllText(FilePath, doc.ToString());
    }
}