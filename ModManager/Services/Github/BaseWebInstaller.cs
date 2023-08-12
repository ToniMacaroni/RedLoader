using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using MelonLoader;
using ModManager.ViewModels;
using SFLoader;

namespace ModManager;

internal abstract class BaseWebInstaller : BaseZipInstaller
{
    protected BaseWebInstaller(string name, InstallationCleaner cleaner) : base(name, cleaner)
    {
    }

    public virtual Version GetLocalVersion()
    {
        return null;
    }

    public override void Install()
    {
        var exePath = InstallationViewModel.GetInstallPath();
        var exeDir = InstallationViewModel.GetDirectoryPath();
        
        if (!Program.ValidateUnityGamePathNonRef(exePath, out exePath))
        {
            // Output Error
            return;
        }

        var selectedVersion = GithubInfoDatabase.SFLoader.GetLatest();
        NewInstall(exeDir, selectedVersion);
    }

    private void NewInstall(string gamePath, string version)
    {
        new Thread(() =>
        {
            DownloadAndInstall(gamePath, version);
        }).Start();
    }

    protected abstract string GetDownloadUrl(string version);

    private void DownloadAndInstall(string destination, string selectedVersion)
    {
        Program.SetCurrentOperation($"Downloading {_name}...");
        var downloadurl = GetDownloadUrl(selectedVersion);
        if (downloadurl == null)
        {
            Program.LogError($"Couldn't find download url for {_name}!");
            return;
        }
        
        var tempPath = TempFileCache.CreateFile();
        try
        {
            Program.WebClient.DownloadFileAsync(new Uri(downloadurl), tempPath);
            while (Program.WebClient.IsBusy)
            { }
        }
        catch (Exception ex)
        {
            Program.LogError(ex.ToString());
            return;
        }

        Program.SetTotalPercentage(50);
        if (Program.Closing)
        {
            return;
        }

        Program.SetCurrentOperation($"Extracting {_name}...");
        try
        {
            Unzip(tempPath, destination);
        }
        catch (Exception ex)
        {
            Program.LogError(ex.ToString());
            return;
        }

        if (Program.Closing)
        {
            return;
        }

        TempFileCache.ClearCache();
        Program.OperationSuccess();
        Program.FinishingMessageBox("Operation was Successful!", MessageBoxButton.OK);
    }
}