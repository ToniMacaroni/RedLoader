using System.Collections.Generic;
using System.IO;
using System.Windows;
using ModManager.ViewModels;

namespace ModManager;

public abstract class InstallationCleaner
{
    protected abstract List<string> FoldersToClear { get; }
    protected abstract List<string> FilesToClear { get; }

    public virtual bool NeedsDialog => true;

    public virtual string CustomMessage => null;
    
    public abstract string Name { get; }
    
    protected string GetFilePath(string fileName)
    {
        return Path.Combine(InstallationViewModel.GetDirectoryPath(), fileName);
    }
    
    public void Clear()
    {
        foreach (var folder in FoldersToClear)
        {
            var folderPath = GetFilePath(folder);
            if (Directory.Exists(folderPath))
                Directory.Delete(folderPath, true);
        }
        
        foreach (var file in FilesToClear)
        {
            var filePath = GetFilePath(file);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
    
    public virtual bool IsInstalled()
    {
        foreach (var folder in FoldersToClear)
        {
            var folderPath = GetFilePath(folder);
            if (Directory.Exists(folderPath))
                return true;
        }
        
        foreach (var file in FilesToClear)
        {
            var filePath = GetFilePath(file);
            if (File.Exists(filePath))
                return true;
        }

        return false;
    }
}

public class MelonCleaner : InstallationCleaner
{
    public override string Name => "MelonLoader";
    
    protected override List<string> FoldersToClear => new()
    {
        "MelonLoader",
        "Mods",
        "UserData",
        "UserLibs"
    };

    protected override List<string> FilesToClear => new()
    {
        "dobby.dll",
        "version.dll",
    };
    
    public override bool IsInstalled()
    {
        return Directory.Exists(GetFilePath("MelonLoader"));
    }
}

public class BieCleaner : InstallationCleaner
{
    public override string Name => "BepInEx";
    
    protected override List<string> FoldersToClear => new()
    {
        "BepInEx",
    };

    protected override List<string> FilesToClear => new()
    {
        "winhttp.dll",
    };
}

public class PartialSfLoaderCleaner : InstallationCleaner
{
    public override string Name => "SFLoader";
    
    protected override List<string> FoldersToClear => new()
    {
        "_SFLoader",
    };

    protected override List<string> FilesToClear => new()
    {
        "dobby.dll",
        "version.dll",
    };
}