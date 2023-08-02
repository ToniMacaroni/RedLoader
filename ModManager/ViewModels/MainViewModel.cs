using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace ModManager.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public static MainViewModel Instance { get; set; } = new();

    public ICommand InstallCommand { get; set; }
    public ICommand UninstallCommand { get; set; }
    public ICommand UninstallBepInExCommand { get; set; }
    public ICommand BrowseCommand { get; set; }

    public string InstallPath
    {
        get => _installPath;
        set
        {
            if (value != _installPath)
            {
                _installPath = value;
                OnPropertyChanged();
            }
        }
    }

    public int CurrentProgress
    {
        get => _currentProgress;
        set
        {
            if (_currentProgress == value) return;
            _currentProgress = value;
            OnPropertyChanged();
        }
    }

    public string CurrentState
    {
        get => _currentState;
        set
        {
            if (value == _currentState) return;
            _currentState = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ProgressVisibility));
        }
    }

    public int MaxProgress
    {
        get => _maxProgress;
        set
        {
            if (value == _maxProgress) return;
            _maxProgress = value;
            OnPropertyChanged();
        }
    }

    public bool CanInstall { get; set; }
    public bool CanUninstall { get; set; }
    public bool CanUninstallBepInEx { get; set; }

    public Visibility ProgressVisibility => CurrentProgress > 0 ? Visibility.Visible : Visibility.Collapsed;

    public MainViewModel()
    {
        InstallCommand = new RelayCommand(Install);
        UninstallCommand = new RelayCommand(Clear);
        UninstallBepInExCommand = new RelayCommand(ClearBepInEx);
        BrowseCommand = new RelayCommand(Browse);
        //InstallPath = Path.Combine(Environment.CurrentDirectory);
        InstallPath = @"F:\SteamLibrary\steamapps\common\SOTF_Melon\SonsOfTheForest.exe";
        RefreshActionAvailability();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void Install(object obj)
    {
        CommandLine.Install(InstallPath);
    }

    private void Clear(object obj)
    {
        CommandLine.Uninstall(InstallPath);
    }

    private void ClearBepInEx(object obj)
    { }

    private void Browse(object obj)
    {
        var dialog = new OpenFileDialog();
        var result = dialog.ShowDialog();
        if (result.HasValue && result.Value)
        {
            InstallPath = dialog.FileName;
        }
        
        RefreshActionAvailability();
    }

    private void RefreshActionAvailability()
    {
        if (!IsPathValid())
        {
            CanInstall = false;
            CanUninstall = false;
            CanUninstallBepInEx = false;
            OnPropertyChanged(nameof(CanInstall));
            OnPropertyChanged(nameof(CanUninstall));
            OnPropertyChanged(nameof(CanUninstallBepInEx));
            return;
        }

        Program.GetCurrentInstallVersion(Path.GetDirectoryName(InstallPath));
        if (Program.CurrentInstalledVersion == null)
        {
            CanInstall = true;
            CanUninstall = false;
            OnPropertyChanged(nameof(CanInstall));
            OnPropertyChanged(nameof(CanUninstall));
            return;
        }

        CanInstall = false;
        CanUninstall = true;
        OnPropertyChanged(nameof(CanInstall));
        OnPropertyChanged(nameof(CanUninstall));
    }

    private bool IsPathValid()
    {
        return InstallPath.EndsWith(".exe") && File.Exists(InstallPath);
    }

    public static void SetProgress(int value)
    {
        if(Instance == null) return;
        Instance.CurrentProgress = value;
    }
    
    public static void SetMaxProgress(int value)
    {
        if(Instance == null) return;
        Instance.MaxProgress = value;
    }
    
    public static void SetState(string value)
    {
        if(Instance == null) return;
        Instance.CurrentState = value;
    }

    public static void OnOperationFinish()
    {
        if(Instance == null) return;
        Instance.RefreshActionAvailability();
        Instance.CurrentState = "SUCCESS!";
        Instance.CurrentProgress = 100;
    }

    private string _installPath;
    private int _currentProgress = 0;
    private string _currentState;
    private int _maxProgress = 100;
}