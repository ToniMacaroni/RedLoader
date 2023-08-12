using ModManager.ViewModels;

namespace ModManager;

internal class DebugInstaller : BaseZipInstaller
{
    private readonly string _zipPath;

    public DebugInstaller(string zipPath, string name, InstallationCleaner cleaner) : base(name, cleaner)
    {
        _zipPath = zipPath;
    }

    public override void Install()
    {
        EnsureValidTarget();
        Unzip(_zipPath, InstallationViewModel.GetDirectoryPath());
        Program.OperationSuccess();
    }
}