using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using MelonLoader;
using ModManager.ViewModels;

namespace ModManager;

internal static class Program
{
    internal static WebClient WebClient;
    internal static Version CurrentInstalledVersion;
    internal static bool Closing = false;
#if DEBUG
    internal static bool RunInstallerUpdateCheck = false;
#else
    internal static bool RunInstallerUpdateCheck = true;
#endif

    static Program()
    {
        AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        ServicePointManager.Expect100Continue = true;
        ServicePointManager.SecurityProtocol =
            SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | (SecurityProtocolType)3072;
        WebClient = new WebClient();
        WebClient.DownloadProgressChanged += (sender, info) =>
        {
            SetCurrentPercentage(info.ProgressPercentage);
            SetTotalPercentage(info.ProgressPercentage / 2);
        };
        Config.Load();
    }

    private static bool FileNameCheck(string[] args)
    {
        var exeFullpath = Process.GetCurrentProcess().MainModule.FileName;
        var exePath = Path.GetDirectoryName(exeFullpath);
        var exeName = Path.GetFileNameWithoutExtension(exeFullpath);
        if (!exeName.EndsWith(".tmp"))
        {
            var tmpExePath = Path.Combine(exePath, exeName + ".tmp.exe");
            if (File.Exists(tmpExePath))
            {
                File.Delete(tmpExePath);
            }

            return false;
        }

        var newExeName = exeName.Substring(0, exeName.Length - 4);
        var newExePath = Path.Combine(exePath, newExeName + ".exe");
        if (File.Exists(newExePath))
        {
            File.Delete(newExePath);
        }

        File.Copy(exeFullpath, newExePath);
        ProcessStartInfo procinfo = new(newExePath);
        if (args != null && args.Length > 0)
        {
            procinfo.Arguments = string.Join(" ",
                args.Where(s => !string.IsNullOrEmpty(s)).Select(it => "\"" + Regex.Replace(it, @"(\\+)$", @"$1$1") + "\""));
        }

        ;
        Process.Start(procinfo);
        Process.GetCurrentProcess().Kill();
        return true;
    }

    internal static void SetCurrentOperation(string op)
    {
        Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            InstallationViewModel.SetState(op);
            // mainForm.Output_Current_Operation.Text = op;
            // mainForm.Output_Current_Operation.ForeColor = System.Drawing.SystemColors.Highlight;
            // mainForm.Output_Current_Progress_Display.Style = MetroFramework.MetroColorStyle.Green;
            // mainForm.Output_Total_Progress_Display.Style = MetroFramework.MetroColorStyle.Green;
            SetCurrentPercentage(0);
        });
    }

    internal static void LogError(string msg)
    {
        TempFileCache.ClearCache();
        OperationError();

        try
        {
            var filePath = Directory.GetCurrentDirectory() + $@"\MLInstaller_{DateTime.Now:yy-M-dd_HH-mm-ss.fff}.log";
            File.WriteAllText(filePath, msg);
            if (Config.HighlightLogFileLocation)
            {
                Process.Start("explorer.exe", $"/select, {filePath}");
            }
#if DEBUG
            FinishingMessageBox(msg, MessageBoxButton.OK);
#else
            FinishingMessageBox($"INTERNAL FAILURE! Please upload the log file \"{filePath}\" when requesting support.", MessageBoxButton.OK);
#endif
        }
        catch (UnauthorizedAccessException)
        {
            FinishingMessageBox(
                "Couldn't create log file! Try running the Installer as Administrator or run the Installer from a different directory.",
                MessageBoxButton.OK);
        }
    }

    internal static void OperationError()
    {
        Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            // mainForm.Output_Current_Operation.Text = "ERROR!";
            // mainForm.Output_Current_Operation.ForeColor = System.Drawing.Color.Red;
            // mainForm.Output_Current_Progress_Display.Style = MetroFramework.MetroColorStyle.Red;
            // mainForm.Output_Total_Progress_Display.Style = MetroFramework.MetroColorStyle.Red;
        });
    }

    internal static void OperationSuccess()
    {
        Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            InstallationViewModel.OnOperationFinish();
            // mainForm.Output_Current_Operation.Text = "SUCCESS!";
            // mainForm.Output_Current_Operation.ForeColor = System.Drawing.Color.Lime;
            // mainForm.Output_Current_Progress_Display.Value = 100;
            // mainForm.Output_Current_Progress_Display.Style = MetroFramework.MetroColorStyle.Green;
            // mainForm.Output_Total_Progress_Display.Style = MetroFramework.MetroColorStyle.Green;
            // mainForm.Output_Current_Progress_Text.Text = mainForm.Output_Current_Progress_Display.Value.ToString();
            // mainForm.Output_Total_Progress_Display.Value = mainForm.Output_Current_Progress_Display.Value;
            // mainForm.Output_Total_Progress_Text.Text = mainForm.Output_Current_Progress_Display.Value.ToString();
        });
    }

    internal static void SetCurrentPercentage(int percentage)
    {
        Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            InstallationViewModel.SetProgress(percentage);
            //mainForm.Output_Current_Progress_Display.Value = percentage;
            //mainForm.Output_Current_Progress_Text.Text = mainForm.Output_Current_Progress_Display.Value.ToString();
        });
    }

    internal static void SetTotalPercentage(int percentage)
    {
        Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            InstallationViewModel.SetMaxProgress(percentage);
            //MainViewModel.SetProgress(percentage);
            //mainForm.Output_Total_Progress_Display.Value = percentage;
            //mainForm.Output_Total_Progress_Text.Text = mainForm.Output_Total_Progress_Display.Value.ToString();
        });
    }

    internal static void FinishingMessageBox(string msg, MessageBoxButton buttons)
    {
        Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            MessageBox.Show(msg, "Installer", buttons);
            SetTotalPercentage(0);
            OperationHandler.CurrentOperation = OperationHandler.Operations.None;
        });
    }

    internal static void GetCurrentInstallVersion(string dirpath)
    {
        var newFilePath = Path.Combine(dirpath, "_SFLoader", "net6", "SFLoader.dll");
        if (!File.Exists(newFilePath))
        {
            return;
        }

        CurrentInstalledVersion = GetFileVersion(newFilePath);
    }

    internal static Version GetFileVersion(string filepath)
    {
        if (!File.Exists(filepath))
        {
            return null;
        }

        var fileVersionInfo = FileVersionInfo.GetVersionInfo(filepath);
        var fileversion = fileVersionInfo.ProductVersion;
        if (string.IsNullOrEmpty(fileversion))
        {
            fileversion = fileVersionInfo.FileVersion;
        }

        if (string.IsNullOrEmpty(fileversion))
        {
            fileversion = "0.0.0.0";
        }

        return new Version(fileversion);
    }

    internal static bool ValidateUnityGamePath(ref string filepath)
    {
        if (string.IsNullOrEmpty(filepath))
        {
            return false;
        }

        var fileExtension = Path.GetExtension(filepath);
        if (string.IsNullOrEmpty(fileExtension) ||
            (!fileExtension.Equals(".exe") && !fileExtension.Equals(".lnk") && !fileExtension.Equals(".url")))
        {
            return false;
        }

        if (fileExtension.Equals(".lnk") || fileExtension.Equals(".url"))
        {
            var newfilepath = GetFilePathFromShortcut(filepath);
            if (string.IsNullOrEmpty(newfilepath) || !newfilepath.EndsWith(".exe"))
            {
                return false;
            }

            filepath = newfilepath;
        }

        // Verify Unity Game

        return true;
    }

    internal static bool ValidateUnityGamePathNonRef(string filepath, out string outFilePath)
    {
        outFilePath = filepath;

        if (string.IsNullOrEmpty(filepath))
        {
            return false;
        }

        var fileExtension = Path.GetExtension(filepath);
        if (string.IsNullOrEmpty(fileExtension) ||
            (!fileExtension.Equals(".exe") && !fileExtension.Equals(".lnk") && !fileExtension.Equals(".url")))
        {
            return false;
        }

        if (fileExtension.Equals(".lnk") || fileExtension.Equals(".url"))
        {
            var newfilepath = GetFilePathFromShortcut(filepath);
            if (string.IsNullOrEmpty(newfilepath) || !newfilepath.EndsWith(".exe"))
            {
                return false;
            }

            outFilePath = newfilepath;
        }

        // Verify Unity Game

        return true;
    }

    internal static bool ValidateZipPath(string filepath)
    {
        if (string.IsNullOrEmpty(filepath))
        {
            return false;
        }

        var fileExtension = Path.GetExtension(filepath);
        if (string.IsNullOrEmpty(fileExtension) || !fileExtension.Equals(".zip"))
        {
            return false;
        }

        return true;
    }

    private static string GetFilePathFromShortcut(string shortcutPath)
    {
        var shortcutExtension = Path.GetExtension(shortcutPath);
        if (shortcutExtension.Equals(".lnk"))
        {
            return GetFilePathFromLnk(shortcutPath);
        }

        if (shortcutExtension.Equals(".url"))
        {
            return GetFilePathFromUrl(shortcutPath);
        }

        return null;
    }

    private static string GetFilePathFromLnk(string shortcutPath)
    {
        throw new NotImplementedException();
        //return ((IWshRuntimeLibrary.IWshShortcut)new IWshRuntimeLibrary.WshShell().CreateShortcut(shortcut_path)).TargetPath;
    }

    private static string GetFilePathFromUrl(string shortcutPath)
    {
        var fileLines = File.ReadAllLines(shortcutPath);
        if (fileLines.Length <= 0)
        {
            return null;
        }

        var urlstring = fileLines.First(x => !string.IsNullOrEmpty(x) && x.StartsWith("URL="));
        if (string.IsNullOrEmpty(urlstring))
        {
            return null;
        }

        urlstring = urlstring.Substring(4);
        if (string.IsNullOrEmpty(urlstring))
        {
            return null;
        }

        if (urlstring.StartsWith("steam://rungameid/"))
        {
            return SteamHandler.GetFilePathFromAppId(urlstring.Substring(18));
        }

        return null;
    }

    private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
    {
        MessageBox.Show((e.ExceptionObject as Exception).ToString());
    }
}