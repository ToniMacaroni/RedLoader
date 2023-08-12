using System.Windows;
using SFLoader;

namespace ModManager;

internal class GithubInstaller : BaseWebInstaller
{
    private readonly GithubInfo _repo;

    public GithubInstaller(GithubInfo repo, string name, InstallationCleaner cleaner) : base(name, cleaner)
    {
        _repo = repo;
    }

    protected override string GetDownloadUrl(string version)
    {
        return _repo.GetDownloadLink(_repo.GetLatest());
    }
}