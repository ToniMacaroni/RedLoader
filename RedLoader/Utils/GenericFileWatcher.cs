using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using File = RedLoader.Preferences.IO.File;

namespace RedLoader;

internal class GenericFileWatcher
{
    public event Action OnFileHasChanged; 

    private FileSystemWatcher FileWatcher = null;
    private readonly FileInfo File = null;

    internal GenericFileWatcher(FileInfo file)
    {
        File = file;
        try
        {
            FileWatcher = new FileSystemWatcher(Path.GetDirectoryName(file.FullName)!, Path.GetFileName(file.FullName))
            {
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };
            FileWatcher.Created += OnFileWatcherTriggered;
            FileWatcher.Changed += OnFileWatcherTriggered;
            FileWatcher.BeginInit();
        }
        catch (Exception ex)
        {
            RLog.Warning("FileSystemWatcher Exception: " + ex);
            FileWatcher = null;
        }
    }

    internal void Destroy()
    {
        if (FileWatcher == null)
            return;
            
        try
        {
            FileWatcher.EndInit();
            FileWatcher.Dispose();
        }
        catch (Exception ex)
        {
            RLog.Warning("FileSystemWatcher Exception: " + ex);
        }
        FileWatcher = null;
    }

    private void OnFileWatcherTriggered(object source, FileSystemEventArgs e)
    {
        OnFileHasChanged?.Invoke();
    }
}

internal class GenericDirectoryWatcher
{
    public event Action<string> OnDirectoryHasChanged; 

    private FileSystemWatcher DirectoryWatcher = null;
    private readonly DirectoryInfo Directory = null;

    internal GenericDirectoryWatcher(DirectoryInfo directory)
    {
        Directory = directory;
        try
        {
            DirectoryWatcher = new FileSystemWatcher(directory.FullName)
            {
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                EnableRaisingEvents = true
            };
            DirectoryWatcher.Created += OnDirectoryWatcherTriggered;
            DirectoryWatcher.Changed += OnDirectoryWatcherTriggered;
            DirectoryWatcher.Deleted += OnDirectoryWatcherTriggered;
            DirectoryWatcher.Renamed += OnDirectoryWatcherTriggered;
            DirectoryWatcher.BeginInit();
        }
        catch (Exception ex)
        {
            RLog.Warning("FileSystemWatcher Exception: " + ex);
            DirectoryWatcher = null;
        }
    }

    internal void Destroy()
    {
        if (DirectoryWatcher == null)
            return;
            
        try
        {
            DirectoryWatcher.EndInit();
            DirectoryWatcher.Dispose();
        }
        catch (Exception ex)
        {
            RLog.Warning("FileSystemWatcher Exception: " + ex);
        }
        DirectoryWatcher = null;
    }

    private void OnDirectoryWatcherTriggered(object source, FileSystemEventArgs e)
    {
        OnDirectoryHasChanged?.Invoke(e.FullPath);
    }
}