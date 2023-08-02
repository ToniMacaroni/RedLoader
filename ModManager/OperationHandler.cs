using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Windows;
using MelonLoader;
using MelonLoader.Tomlyn.Model;
using ModManager.Libs;
using ModManager.Libs.Tomlyn;

namespace ModManager;

internal static class OperationHandler
{
    private static readonly string[] ProxyNames = { "version", "winmm", "winhttp" };
    internal static Operations CurrentOperation = Operations.None;

    internal static string CurrentOperationName
    {
        get
        {
            string opname = null;
            switch (CurrentOperation)
            {
                case Operations.Downgrade:
                    opname = "DOWNGRADE";
                    break;
                case Operations.Update:
                    opname = "UPDATE";
                    break;
                case Operations.Uninstall:
                    opname = "UN-INSTALL";
                    break;
                case Operations.Reinstall:
                    opname = "RE-INSTALL";
                    break;
                case Operations.Install:
                case Operations.InstallerUpdate:
                case Operations.None:
                default:
                    opname = "INSTALL";
                    break;
            }

            return opname;
        }
    }

    internal static void Automated_Install(string destination, string selectedVersion, bool isX86, bool legacyVersion)
    {
        Program.SetCurrentOperation("Downloading MelonLoader...");
        var downloadurl = Config.DownloadMelonLoader + "/" + selectedVersion + "/MelonLoader." + (!legacyVersion && isX86 ? "x86" : "x64") +
                          ".zip";
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

        var repoHashUrl = Config.DownloadMelonLoader + "/" + selectedVersion + "/MelonLoader.x64.sha512";
        string repoHash = null;
        try
        {
            repoHash = Program.WebClient.DownloadString(repoHashUrl);
        }
        catch
        {
            repoHash = null;
        }

        if (string.IsNullOrEmpty(repoHash))
        {
            Program.LogError("Failed to get SHA512 Hash from Repo!");
            return;
        }

        if (Program.Closing)
        {
            return;
        }

        var sha512 = new SHA512Managed();
        var checksum = sha512.ComputeHash(File.ReadAllBytes(tempPath));
        if (checksum == null || checksum.Length <= 0)
        {
            Program.LogError("Failed to get SHA512 Hash from Temp File!");
            return;
        }

        var fileHash = BitConverter.ToString(checksum).Replace("-", string.Empty);
        if (string.IsNullOrEmpty(fileHash))
        {
            Program.LogError("Failed to get SHA512 Hash from Temp File!");
            return;
        }

        if (!fileHash.Equals(repoHash))
        {
            Program.LogError("SHA512 Hash from Temp File does not match Repo Hash!");
            return;
        }

        Program.SetCurrentOperation("Extracting MelonLoader...");
        try
        {
            var melonLoaderFolder = Path.Combine(destination, "MelonLoader");
            if (Directory.Exists(melonLoaderFolder))
            {
                ThreadHandler.RecursiveFuncRun(delegate(ThreadHandler.RecursiveFuncRecurse recurse)
                {
                    try
                    {
                        Directory.Delete(melonLoaderFolder, true);
                    }
                    catch (Exception ex)
                    {
                        if (!ex.GetType().IsAssignableFrom(typeof(UnauthorizedAccessException))
                            && !ex.GetType().IsAssignableFrom(typeof(IOException)))
                        {
                            throw ex;
                        }

                        var result = MessageBox.Show(
                            "Unable to remove Existing MelonLoader Folder! Make sure the Unity Game is not running or try running the Installer as Administrator.",
                            "Installer", MessageBoxButton.OKCancel);
                        if (result == MessageBoxResult.OK)
                        {
                            recurse.Invoke();
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                });
            }

            string proxyPath = null;
            if (GetExistingProxyPath(destination, out proxyPath))
            {
                ThreadHandler.RecursiveFuncRun(delegate(ThreadHandler.RecursiveFuncRecurse recurse)
                {
                    try
                    {
                        File.Delete(proxyPath);
                    }
                    catch (Exception ex)
                    {
                        if (!ex.GetType().IsAssignableFrom(typeof(UnauthorizedAccessException))
                            && !ex.GetType().IsAssignableFrom(typeof(IOException)))
                        {
                            throw ex;
                        }

                        var result = MessageBox.Show(
                            "Unable to remove Existing Proxy Module! Make sure the Unity Game is not running or try running the Installer as Administrator.",
                            "Installer", MessageBoxButton.OKCancel);
                        if (result == MessageBoxResult.OK)
                        {
                            recurse.Invoke();
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                });
            }

            using var stream = new FileStream(tempPath, FileMode.Open, FileAccess.Read);
            using var zip = new ZipArchive(stream);
            var totalEntryCount = zip.Entries.Count;
            for (var i = 0; i < totalEntryCount; i++)
            {
                if (Program.Closing)
                {
                    break;
                }

                var percentage = i / totalEntryCount * 100;
                Program.SetCurrentPercentage(percentage);
                Program.SetTotalPercentage(50 + percentage / 2);
                var entry = zip.Entries[i];
                var fullPath = Path.Combine(destination, entry.FullName);
                if (!fullPath.StartsWith(destination))
                {
                    throw new IOException("Extracting Zip entry would have resulted in a file outside the specified destination directory.");
                }

                var filename = Path.GetFileName(fullPath);
                if (filename.Length != 0)
                {
                    if (!legacyVersion && filename.Equals("version.dll"))
                    {
                        foreach (var proxyname in ProxyNames)
                        {
                            var newProxyPath = Path.Combine(destination, proxyname + ".dll");
                            if (File.Exists(newProxyPath))
                            {
                                continue;
                            }

                            fullPath = newProxyPath;
                            break;
                        }
                    }

                    var directorypath = Path.GetDirectoryName(fullPath);
                    if (!Directory.Exists(directorypath))
                    {
                        Directory.CreateDirectory(directorypath);
                    }

                    using var targetStream = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Write);
                    using var entryStream = entry.Open();
                    ThreadHandler.RecursiveFuncRun(delegate(ThreadHandler.RecursiveFuncRecurse recurse)
                    {
                        try
                        {
                            entryStream.CopyTo(targetStream);
                        }
                        catch (Exception ex)
                        {
                            if (!ex.GetType().IsAssignableFrom(typeof(UnauthorizedAccessException))
                                && !ex.GetType().IsAssignableFrom(typeof(IOException)))
                            {
                                throw ex;
                            }

                            var result = MessageBox.Show(
                                $"Couldn't extract file {filename}! Make sure the Unity Game is not running or try running the Installer as Administrator.",
                                "Installer", MessageBoxButton.OKCancel);
                            if (result == MessageBoxResult.OK)
                            {
                                recurse.Invoke();
                            }
                            else
                            {
                                throw ex;
                            }
                        }
                    });
                    continue;
                }

                if (entry.Length != 0)
                {
                    throw new IOException("Zip entry name ends in directory separator character but contains data.");
                }

                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }
            }

            ThreadHandler.RecursiveFuncRun(delegate(ThreadHandler.RecursiveFuncRecurse recurse)
            {
                try
                {
                    DowngradeMelonPreferences(destination, legacyVersion);
                }
                catch (Exception ex)
                {
                    if (!ex.GetType().IsAssignableFrom(typeof(UnauthorizedAccessException))
                        && !ex.GetType().IsAssignableFrom(typeof(IOException)))
                    {
                        throw ex;
                    }

                    var result = MessageBox.Show(
                        "Unable to Downgrade MelonLoader Preferences! Make sure the Unity Game is not running or try running the Installer as Administrator.",
                        "Installer", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        recurse.Invoke();
                    }
                    else
                    {
                        throw ex;
                    }
                }
            });
            ExtraDirectoryChecks(destination);
            ThreadHandler.RecursiveFuncRun(delegate(ThreadHandler.RecursiveFuncRecurse recurse)
            {
                try
                {
                    ExtraCleanupCheck(destination);
                }
                catch (Exception ex)
                {
                    if (!ex.GetType().IsAssignableFrom(typeof(UnauthorizedAccessException))
                        && !ex.GetType().IsAssignableFrom(typeof(IOException)))
                    {
                        throw ex;
                    }

                    var result = MessageBox.Show(
                        "Couldn't do Extra File Cleanup! Make sure the Unity Game is not running or try running the Installer as Administrator.",
                        "Installer", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        recurse.Invoke();
                    }
                    else
                    {
                        throw ex;
                    }
                }
            });
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
        Program.FinishingMessageBox(CurrentOperationName + " was Successful!", MessageBoxButton.OK);
    }

    internal static void ManualZip_Install(string zipPath, string destination)
    {
        Program.SetCurrentOperation("Extracting Zip Archive...");
        try
        {
            var melonLoaderFolder = Path.Combine(destination, "MelonLoader");
            if (Directory.Exists(melonLoaderFolder))
            {
                ThreadHandler.RecursiveFuncRun(delegate(ThreadHandler.RecursiveFuncRecurse recurse)
                {
                    try
                    {
                        Directory.Delete(melonLoaderFolder, true);
                    }
                    catch (Exception ex)
                    {
                        if (!ex.GetType().IsAssignableFrom(typeof(UnauthorizedAccessException))
                            && !ex.GetType().IsAssignableFrom(typeof(IOException)))
                        {
                            throw ex;
                        }

                        var result = MessageBox.Show(
                            "Unable to remove Existing MelonLoader Folder! Make sure the Unity Game is not running or try running the Installer as Administrator.",
                            "Installer", MessageBoxButton.OKCancel);
                        if (result == MessageBoxResult.OK)
                        {
                            recurse.Invoke();
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                });
            }

            string proxyPath = null;
            if (GetExistingProxyPath(destination, out proxyPath))
            {
                ThreadHandler.RecursiveFuncRun(delegate(ThreadHandler.RecursiveFuncRecurse recurse)
                {
                    try
                    {
                        File.Delete(proxyPath);
                    }
                    catch (Exception ex)
                    {
                        if (!ex.GetType().IsAssignableFrom(typeof(UnauthorizedAccessException))
                            && !ex.GetType().IsAssignableFrom(typeof(IOException)))
                        {
                            throw ex;
                        }

                        var result = MessageBox.Show(
                            "Unable to remove Existing Proxy Module! Make sure the Unity Game is not running or try running the Installer as Administrator.",
                            "Installer", MessageBoxButton.OKCancel);
                        if (result == MessageBoxResult.OK)
                        {
                            recurse.Invoke();
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                });
            }

            using var stream = new FileStream(zipPath, FileMode.Open, FileAccess.Read);
            using var zip = new ZipArchive(stream);
            var totalEntryCount = zip.Entries.Count;
            for (var i = 0; i < totalEntryCount; i++)
            {
                if (Program.Closing)
                {
                    break;
                }

                var percentage = i / totalEntryCount * 100;
                Program.SetCurrentPercentage(percentage);
                Program.SetTotalPercentage(percentage);
                var entry = zip.Entries[i];
                var fullPath = Path.Combine(destination, entry.FullName);
                if (!fullPath.StartsWith(destination))
                {
                    throw new IOException("Extracting Zip entry would have resulted in a file outside the specified destination directory.");
                }

                var filename = Path.GetFileName(fullPath);
                if (filename.Length != 0)
                {
                    var directorypath = Path.GetDirectoryName(fullPath);
                    if (!Directory.Exists(directorypath))
                    {
                        Directory.CreateDirectory(directorypath);
                    }

                    using var targetStream = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Write);
                    using var entryStream = entry.Open();
                    ThreadHandler.RecursiveFuncRun(delegate(ThreadHandler.RecursiveFuncRecurse recurse)
                    {
                        try
                        {
                            entryStream.CopyTo(targetStream);
                        }
                        catch (Exception ex)
                        {
                            if (!ex.GetType().IsAssignableFrom(typeof(UnauthorizedAccessException))
                                && !ex.GetType().IsAssignableFrom(typeof(IOException)))
                            {
                                throw ex;
                            }

                            var result = MessageBox.Show(
                                $"Couldn't extract file {filename}! Make sure the Unity Game is not running or try running the Installer as Administrator.",
                                "Installer", MessageBoxButton.OKCancel);
                            if (result == MessageBoxResult.OK)
                            {
                                recurse.Invoke();
                            }
                            else
                            {
                                throw ex;
                            }
                        }
                    });
                    continue;
                }

                if (entry.Length != 0)
                {
                    throw new IOException("Zip entry name ends in directory separator character but contains data.");
                }

                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }
            }

            ExtraDirectoryChecks(destination);
            ThreadHandler.RecursiveFuncRun(delegate(ThreadHandler.RecursiveFuncRecurse recurse)
            {
                try
                {
                    ExtraCleanupCheck(destination);
                }
                catch (Exception ex)
                {
                    if (!ex.GetType().IsAssignableFrom(typeof(UnauthorizedAccessException))
                        && !ex.GetType().IsAssignableFrom(typeof(IOException)))
                    {
                        throw ex;
                    }

                    var result = MessageBox.Show(
                        "Couldn't do Extra File Cleanup! Make sure the Unity Game is not running or try running the Installer as Administrator.",
                        "Installer", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        recurse.Invoke();
                    }
                    else
                    {
                        throw ex;
                    }
                }
            });
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

        Program.OperationSuccess();
        Program.FinishingMessageBox(CurrentOperationName + " was Successful!", MessageBoxButton.OK);
    }

    internal static void Uninstall(string destination)
    {
        Program.SetCurrentOperation("Uninstalling MelonLoader...");
        try
        {
            var melonLoaderFolder = Path.Combine(destination, "MelonLoader");
            if (Directory.Exists(melonLoaderFolder))
            {
                ThreadHandler.RecursiveFuncRun(delegate(ThreadHandler.RecursiveFuncRecurse recurse)
                {
                    try
                    {
                        Directory.Delete(melonLoaderFolder, true);
                    }
                    catch (Exception ex)
                    {
                        if (!ex.GetType().IsAssignableFrom(typeof(UnauthorizedAccessException))
                            && !ex.GetType().IsAssignableFrom(typeof(IOException)))
                        {
                            throw ex;
                        }

                        var result = MessageBox.Show(
                            "Unable to remove Existing MelonLoader Folder! Make sure the Unity Game is not running or try running the Installer as Administrator.",
                            "Installer", MessageBoxButton.OKCancel);
                        if (result == MessageBoxResult.OK)
                        {
                            recurse.Invoke();
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                });
            }

            string proxyPath = null;
            if (GetExistingProxyPath(destination, out proxyPath))
            {
                ThreadHandler.RecursiveFuncRun(delegate(ThreadHandler.RecursiveFuncRecurse recurse)
                {
                    try
                    {
                        File.Delete(proxyPath);
                    }
                    catch (Exception ex)
                    {
                        if (!ex.GetType().IsAssignableFrom(typeof(UnauthorizedAccessException))
                            && !ex.GetType().IsAssignableFrom(typeof(IOException)))
                        {
                            throw ex;
                        }

                        var result = MessageBox.Show(
                            "Unable to remove Existing Proxy Module! Make sure the Unity Game is not running or try running the Installer as Administrator.",
                            "Installer", MessageBoxButton.OKCancel);
                        if (result == MessageBoxResult.OK)
                        {
                            recurse.Invoke();
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                });
            }

            ThreadHandler.RecursiveFuncRun(delegate(ThreadHandler.RecursiveFuncRecurse recurse)
            {
                try
                {
                    ExtraCleanupCheck(destination);
                }
                catch (Exception ex)
                {
                    if (!ex.GetType().IsAssignableFrom(typeof(UnauthorizedAccessException))
                        && !ex.GetType().IsAssignableFrom(typeof(IOException)))
                    {
                        throw ex;
                    }

                    var result = MessageBox.Show(
                        "Couldn't do Extra File Cleanup! Make sure the Unity Game is not running or try running the Installer as Administrator.",
                        "Installer", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        recurse.Invoke();
                    }
                    else
                    {
                        throw ex;
                    }
                }
            });

            var dobbyPath = Path.Combine(destination, "dobby.dll");
            if (File.Exists(dobbyPath))
            {
                ThreadHandler.RecursiveFuncRun(delegate(ThreadHandler.RecursiveFuncRecurse recurse)
                {
                    try
                    {
                        File.Delete(dobbyPath);
                    }
                    catch (Exception ex)
                    {
                        if (!ex.GetType().IsAssignableFrom(typeof(UnauthorizedAccessException))
                            && !ex.GetType().IsAssignableFrom(typeof(IOException)))
                        {
                            throw ex;
                        }

                        var result = MessageBox.Show(
                            "Unable to remove dobby.dll! Make sure the Unity Game is not running or try running the Installer as Administrator.",
                            "Installer", MessageBoxButton.OKCancel);
                        if (result == MessageBoxResult.OK)
                        {
                            recurse.Invoke();
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                });
            }
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

        Program.CurrentInstalledVersion = null;
        Program.OperationSuccess();
        Program.FinishingMessageBox(CurrentOperationName + " was Successful!", MessageBoxButton.OK);
    }

    private static bool GetExistingProxyPath(string destination, out string proxyPath)
    {
        proxyPath = null;
        foreach (var proxy in ProxyNames)
        {
            var newProxyPath = Path.Combine(destination, proxy + ".dll");
            if (!File.Exists(newProxyPath))
            {
                continue;
            }

            var fileinfo = FileVersionInfo.GetVersionInfo(newProxyPath);
            if (fileinfo == null)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(fileinfo.LegalCopyright) && fileinfo.LegalCopyright.Contains("Microsoft"))
            {
                continue;
            }

            proxyPath = newProxyPath;
            break;
        }

        return !string.IsNullOrEmpty(proxyPath);
    }

    private static void DowngradeMelonPreferences(string destination, bool legacyVersion)
    {
        if (!legacyVersion || Program.CurrentInstalledVersion == null || Program.CurrentInstalledVersion.CompareTo(new Version("0.3.0")) < 0)
        {
            return;
        }

        var userdatapath = Path.Combine(destination, "UserData");
        if (!Directory.Exists(userdatapath))
        {
            return;
        }

        var oldfilepath = Path.Combine(userdatapath, "MelonPreferences.cfg");
        if (!File.Exists(oldfilepath))
        {
            return;
        }

        var filestr = File.ReadAllText(oldfilepath);
        if (string.IsNullOrEmpty(filestr))
        {
            return;
        }

        var docsyn = Toml.Parse(filestr);
        if (docsyn == null)
        {
            return;
        }

        var model = docsyn.ToModel();
        if (model.Count <= 0)
        {
            return;
        }

        var newfilepath = Path.Combine(userdatapath, "modprefs.ini");
        if (File.Exists(newfilepath))
        {
            File.Delete(newfilepath);
        }

        var iniFile = new IniFile(newfilepath);
        foreach (var keypair in model)
        {
            var categoryName = keypair.Key;
            var tbl = (TomlTable)keypair.Value;
            if (tbl.Count <= 0)
            {
                continue;
            }

            foreach (var tblkeypair in tbl)
            {
                var name = tblkeypair.Key;
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                var obj = TomlObject.ToTomlObject(tblkeypair.Value);
                if (obj == null)
                {
                    continue;
                }

                switch (obj.Kind)
                {
                    case ObjectKind.String:
                        iniFile.SetString(categoryName, name, ((TomlString)obj).Value);
                        break;
                    case ObjectKind.Boolean:
                        iniFile.SetBool(categoryName, name, ((TomlBoolean)obj).Value);
                        break;
                    case ObjectKind.Integer:
                        iniFile.SetInt(categoryName, name, (int)((TomlInteger)obj).Value);
                        break;
                    case ObjectKind.Float:
                        iniFile.SetFloat(categoryName, name, (float)((TomlFloat)obj).Value);
                        break;
                }
            }
        }

        File.Delete(oldfilepath);
    }

    private static void ExtraDirectoryChecks(string destination)
    {
        var pluginsDirectory = Path.Combine(destination, "MelonLoader", "Plugins");
        if (!Directory.Exists(pluginsDirectory))
        {
            Directory.CreateDirectory(pluginsDirectory);
        }

        var modsDirectory = Path.Combine(destination, "Mods");
        if (!Directory.Exists(modsDirectory))
        {
            Directory.CreateDirectory(modsDirectory);
        }

        var userdataDirectory = Path.Combine(destination, "UserData");
        if (!Directory.Exists(userdataDirectory))
        {
            Directory.CreateDirectory(userdataDirectory);
        }
    }

    private static void ExtraCleanupCheck(string destination)
    {
        var mainDll = Path.Combine(destination, "MelonLoader.dll");
        if (File.Exists(mainDll))
        {
            File.Delete(mainDll);
        }

        mainDll = Path.Combine(destination, "Mods", "MelonLoader.dll");
        if (File.Exists(mainDll))
        {
            File.Delete(mainDll);
        }

        mainDll = Path.Combine(destination, "Plugins", "MelonLoader.dll");
        if (File.Exists(mainDll))
        {
            File.Delete(mainDll);
        }

        mainDll = Path.Combine(destination, "UserData", "MelonLoader.dll");
        if (File.Exists(mainDll))
        {
            File.Delete(mainDll);
        }

        var main2Dll = Path.Combine(destination, "MelonLoader.ModHandler.dll");
        if (File.Exists(main2Dll))
        {
            File.Delete(main2Dll);
        }

        main2Dll = Path.Combine(destination, "Mods", "MelonLoader.ModHandler.dll");
        if (File.Exists(main2Dll))
        {
            File.Delete(main2Dll);
        }

        main2Dll = Path.Combine(destination, "Plugins", "MelonLoader.ModHandler.dll");
        if (File.Exists(main2Dll))
        {
            File.Delete(main2Dll);
        }

        main2Dll = Path.Combine(destination, "UserData", "MelonLoader.ModHandler.dll");
        if (File.Exists(main2Dll))
        {
            File.Delete(main2Dll);
        }

        var logsPath = Path.Combine(destination, "Logs");
        if (Directory.Exists(logsPath))
        {
            Directory.Delete(logsPath, true);
        }
    }

    internal enum Operations
    {
        None,
        InstallerUpdate,
        Install,
        Uninstall,
        Reinstall,
        Update,
        Downgrade
    }
}