using System;
using System.IO;
using System.Threading;
using System.Windows;
using MelonLoader;
using SFLoader;

namespace ModManager;

internal static class CommandLine
{
    internal static bool IsCmd = false;
    internal static bool IsSilent;
    internal static int CmdMode;
    internal static string ExePath;
    internal static string ZipPath = null;
    internal static string RequestedVersion = null;
    internal static bool AutoDetectArch = true;
    internal static bool Requested32Bit;

    public static void Install(string exePath)
    {
        if (!Program.ValidateUnityGamePathNonRef(exePath, out exePath))
        {
            // Output Error
            return;
        }

        Program.GetCurrentInstallVersion(Path.GetDirectoryName(exePath));
        if (!string.IsNullOrEmpty(ZipPath))
        {
            var returnval = 0;
            InstallFromZip(ref returnval);
            return;
        }

        //string selected_version = "v0.0.0.0";
        var selectedVersion = GithubInfoDatabase.SFLoader.GetLatest();
        if (Program.CurrentInstalledVersion == null)
        {
            OperationHandler.CurrentOperation = OperationHandler.Operations.Install;
        }
        else
        {
            var selectedVer = new Version(selectedVersion);
            var compareVer = selectedVer.CompareTo(Program.CurrentInstalledVersion);
            if (compareVer < 0)
            {
                OperationHandler.CurrentOperation = OperationHandler.Operations.Downgrade;
            }
            else if (compareVer > 0)
            {
                OperationHandler.CurrentOperation = OperationHandler.Operations.Update;
            }
            else
            {
                OperationHandler.CurrentOperation = OperationHandler.Operations.Reinstall;
            }
        }

        new Thread(() => { OperationHandler.Automated_Install(Path.GetDirectoryName(exePath), selectedVersion, false, false); }).Start();
    }

    public static void Uninstall(string exePath)
    {
        if (!Program.ValidateUnityGamePathNonRef(exePath, out exePath))
        {
            // Output Error
            return;
        }

        var folderpath = Path.GetDirectoryName(exePath);
        Program.GetCurrentInstallVersion(folderpath);
        if (Program.CurrentInstalledVersion == null)
        {
            MessageBox.Show("No Version");
            // Output Error
            return;
        }

        OperationHandler.CurrentOperation = OperationHandler.Operations.Uninstall;
        MessageBox.Show("Uninstalling");
        OperationHandler.Uninstall(folderpath);
    }

    private static void InstallFromZip(ref int returnval)
    {
        if (!Program.ValidateZipPath(ZipPath))
        {
            // Output Error
            return;
        }

        OperationHandler.ManualZip_Install(ZipPath, Path.GetDirectoryName(ExePath));
    }
}