using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;

namespace ModManager;

internal abstract class BaseZipInstaller : BaseInstaller
{
    private readonly InstallationCleaner _cleaner;

    protected BaseZipInstaller(string name, InstallationCleaner cleaner) : base(name)
    {
        _cleaner = cleaner;
    }

    protected virtual void Unzip(string zipFile, string destination)
    {
        EnsureValidTarget();

        var fileInfo = new FileInfo(zipFile);
        
        if(fileInfo.Length < 100)
        {
            Program.LogError($"Zip file {zipFile} is empty!");
            return;
        }

        using var stream = new FileStream(zipFile, FileMode.Open, FileAccess.Read);
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
                var directorypath = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(directorypath))
                {
                    Directory.CreateDirectory(directorypath!);
                }

                using var targetStream = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Write);
                using var entryStream = entry.Open();
                Recurser.RecursiveFuncRun(delegate(Recurser.RecursiveFuncRecurse recurse)
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
    }
    
    protected virtual void EnsureValidTarget()
    {
        if (_cleaner == null)
            return;
        
        _cleaner.Clear();
    }
}