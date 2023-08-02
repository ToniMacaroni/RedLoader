using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using MelonLoader;
using Microsoft.Win32;

namespace ModManager.ViewModels;

public class InstallationViewModel : INotifyPropertyChanged
{
    public static InstallationViewModel Instance { get; set; } = new();

    public ICommand InstallCommand { get; set; }
    public ICommand UninstallCommand { get; set; }
    public ICommand UninstallBepInExCommand { get; set; }
    public ICommand UpdateCommand { get; set; }
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

    public string UpdateText
    {
        get => _updateText;
        set
        {
            if (value == _updateText) return;
            _updateText = value;
            OnPropertyChanged();
        }
    }

    public bool CanInstall { get; set; }
    public bool CanUninstall { get; set; }
    public bool CanUninstallBepInEx { get; set; }
    
    public bool CanUpdate { get; set; }

    public Visibility ProgressVisibility => CurrentProgress > 0 ? Visibility.Visible : Visibility.Collapsed;

    public InstallationViewModel()
    {
        InstallCommand = new RelayCommand(Install);
        UninstallCommand = new RelayCommand(Clear);
        UninstallBepInExCommand = new RelayCommand(ClearBepInEx);
        UpdateCommand = new RelayCommand(Update);
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
    {
        var gameFolder = Path.GetDirectoryName(InstallPath)!;
        var bepinexFolder = Path.Combine(gameFolder, "BepInEx");
        var winhttpDll = Path.Combine(gameFolder, "winhttp.dll");

        var messageResult =
            MessageBox.Show(
                "This will completely remove BepInEx. Make sure to backup your mods to another location if you want to keep them. Continue?",
                "Warning", MessageBoxButton.YesNo);
        if(messageResult != MessageBoxResult.Yes) return;

        if (Directory.Exists(bepinexFolder))
        {
            Directory.Delete(bepinexFolder, true);
        }
        
        if (File.Exists(winhttpDll))
        {
            File.Delete(winhttpDll);
        }
        
        RefreshActionAvailability();

        CurrentState = "BepInEx uninstalled";
    }

    private void Update(object obj)
    {
        Clear(null);
    }

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
        void Refresh()
        {
            OnPropertyChanged(nameof(CanInstall));
            OnPropertyChanged(nameof(CanUninstall));
            OnPropertyChanged(nameof(CanUninstallBepInEx));
            OnPropertyChanged(nameof(CanUpdate));
        }
        
        if (!IsPathValid())
        {
            // No actions
            CanInstall = false;
            CanUninstall = false;
            CanUninstallBepInEx = false;
            CanUpdate = false;
            Refresh();
            CurrentState = "Select game executable...";
            return;
        }

        CurrentState = "Select action...";

        Program.GetCurrentInstallVersion(Path.GetDirectoryName(InstallPath));
        if (Program.CurrentInstalledVersion == null)
        {
            // Install
            CanInstall = true;
            CanUninstall = false;
            CanUpdate = false;
            Refresh();
            RefreshBepInExActionAvailability();
            return;
        }

        // Uninstall or Update
        CanInstall = false;
        CanUninstall = true;
        RefreshUpdateAction();
        Refresh();
        RefreshBepInExActionAvailability();
    }

    private void RefreshBepInExActionAvailability()
    {
        var gameFolder = Path.GetDirectoryName(InstallPath)!;
        var bepinexFolder = Path.Combine(gameFolder, "BepInEx");
        var winhttpDll = Path.Combine(gameFolder, "winhttp.dll");
        
        CanUninstallBepInEx = Directory.Exists(bepinexFolder) || File.Exists(winhttpDll);
        OnPropertyChanged(nameof(CanUninstallBepInEx));
    }

    private void RefreshUpdateAction()
    {
        if (Releases.All == null || Releases.All.Count == 0)
        {
            Releases.RequestLists();
        }
        
        var localVersion = "v"+Program.CurrentInstalledVersion;
        var remoteVersion = Releases.All![0];

        CanUpdate = localVersion != remoteVersion;
        UpdateText = $"Update ({localVersion}) -> ({remoteVersion})";
        
        OnPropertyChanged(nameof(CanUpdate));
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
    private string _currentState = "Select the game executable!";
    private int _maxProgress = 100;
    private string _updateText = "Update";
}