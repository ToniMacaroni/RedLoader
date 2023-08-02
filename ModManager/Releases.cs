using System.Collections.Generic;
using MelonLoader.LightJson;
using ModManager;

namespace MelonLoader;

internal static class Releases
{
    private static List<string> Official = new();
    private static List<string> All = new();

    internal static void RequestLists()
    {
        if (IsInitialised())
            return;
        
        Program.WebClient.Headers.Clear();
        Program.WebClient.Headers.Add("User-Agent", "Unity web player");
        string response = null;
        try
        {
            response = Program.WebClient.DownloadString(Config.RepoApiMelonLoader);
        }
        catch
        {
            response = null;
        }

        if (string.IsNullOrEmpty(response))
        {
            return;
        }

        if (Program.Closing)
        {
            return;
        }

        var data = JsonValue.Parse(response).AsJsonArray;
        if (data.Count <= 0)
        {
            return;
        }

        Official.Clear();
        All.Clear();
        foreach (var release in data)
        {
            var assets = release["assets"].AsJsonArray;
            if (assets.Count <= 0)
            {
                continue;
            }

            var version = release["tag_name"].AsString;
            if (version.StartsWith("v0.2") || version.StartsWith("v0.1"))
            {
                continue;
            }

            if (!release["prerelease"].AsBoolean)
            {
                Official.Add(version);
            }

            All.Add(version);
        }

        Official.Sort();
        Official.Reverse();
        All.Sort();
        All.Reverse();
    }

    public static string GetLatest()
    {
        RequestLists();
        return All[0];
    }

    public static List<string> GetReleases()
    {
        RequestLists();
        return All;
    }

    public static bool IsInitialised()
    {
        return All != null && All.Count > 0;
    }
}