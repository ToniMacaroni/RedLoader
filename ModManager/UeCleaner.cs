using System.Collections.Generic;

namespace ModManager;

internal class UeCleaner : InstallationCleaner
{
    protected override List<string> FoldersToClear => new()
    {
        "Mods\\sinai-dev-UnityExplorer"
    };
    
    protected override List<string> FilesToClear => new()
    {
        "Mods\\UnityExplorer.SFLoader.dll",
    };
    
    public override string Name => "Unity Explorer";

    public override bool NeedsDialog => false;
}