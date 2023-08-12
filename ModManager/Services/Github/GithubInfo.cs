using System.Collections.Generic;
using System.Windows;
using MelonLoader.LightJson;
using ModManager;

namespace SFLoader;

internal class GithubInfo
{
    private readonly string _repo;
    private readonly string _githubFileName;

    public string APIPath => $"https://api.github.com/repos/{Repo}/releases";
    public string DownloadPath => $"https://github.com/{Repo}/releases/download";

    public string Repo => _repo;

    private List<string> _releases = new();

    public GithubInfo(string repo, string githubFileName)
    {
        _repo = repo;
        _githubFileName = githubFileName;
    }
    
    public string GetDownloadLink(string version)
    {
        return $"{DownloadPath}/{version}/{_githubFileName}";
    }
    
    internal void Fetch()
    {
        if (IsInitialised())
            return;
        
        Program.WebClient.Headers.Clear();
        Program.WebClient.Headers.Add("User-Agent", "Unity web player");
        string response = null;
        try
        {
            response = Program.WebClient.DownloadString(APIPath);
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

        _releases.Clear();
        
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

            // if (!release["prerelease"].AsBoolean)
            // {
            //     _official.Add(version);
            // }

            _releases.Add(version);
        }

        _releases.Sort();
        _releases.Reverse();
    }

    public string GetLatest()
    {
        Fetch();
        if (_releases.Count <= 0)
        {
            return null;
        }
        return _releases[0];
    }

    public List<string> GetReleases()
    {
        Fetch();
        return _releases;
    }

    public bool IsInitialised()
    {
        return _releases != null && _releases.Count > 0;
    }
}