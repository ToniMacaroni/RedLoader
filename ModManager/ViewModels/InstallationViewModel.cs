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

    public bool CanInstall
    {
        get => _canInstall;
        set
        {
            if (value == _canInstall) return;
            _canInstall = value;
            OnPropertyChanged();
        }
    }

    public bool CanUninstall
    {
        get => _canUninstall;
        set
        {
            if (value == _canUninstall) return;
            _canUninstall = value;
            OnPropertyChanged();
        }
    }

    public bool CanUninstallBepInEx
    {
        get => _canUninstallBepInEx;
        set
        {
            if (value == _canUninstallBepInEx) return;
            _canUninstallBepInEx = value;
            OnPropertyChanged();
        }
    }

    public bool CanUpdate
    {
        get => _canUpdate;
        set
        {
            if (value == _canUpdate) return;
            _canUpdate = value;
            OnPropertyChanged();
        }
    }

    public string InstallText
    {
        get => _installText;
        set
        {
            if (value == _installText) return;
            _installText = value;
            OnPropertyChanged();
        }
    }

    public Visibility ProgressVisibility => CurrentProgress > 0 ? Visibility.Visible : Visibility.Collapsed;

    public InstallationViewModel()
    {
        InstallCommand = new RelayCommand(Install);
        UninstallCommand = new RelayCommand(Clear);
        UninstallBepInExCommand = new RelayCommand(ClearBepInEx);
        UpdateCommand = new RelayCommand(Update);
        BrowseCommand = new RelayCommand(Browse);
        //InstallPath = Path.Combine(Environment.CurrentDirectory);
        InstallPath = PathTools.GetGamePath() ?? "Select SonsOfTheForest.exe";
        RefreshActionAvailability();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void Install(object obj)
    {
        HideAllActions();
        CommandLine.Install(InstallPath);
    }

    private void Clear(object obj)
    {
        HideAllActions();
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
        if (!IsPathValid())
        {
            // No actions
            CanInstall = false;
            CanUninstall = false;
            CanUninstallBepInEx = false;
            CanUpdate = false;
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
            InstallText = $"Install ({Releases.GetLatest()})";
            RefreshBepInExActionAvailability();
            return;
        }

        // Uninstall or Update
        CanInstall = false;
        CanUninstall = true;
        RefreshUpdateAction();
        RefreshBepInExActionAvailability();
    }

    private void RefreshBepInExActionAvailability()
    {
        var gameFolder = Path.GetDirectoryName(InstallPath)!;
        var bepinexFolder = Path.Combine(gameFolder, "BepInEx");
        var winhttpDll = Path.Combine(gameFolder, "winhttp.dll");
        
        CanUninstallBepInEx = Directory.Exists(bepinexFolder) || File.Exists(winhttpDll);
    }

    private void RefreshUpdateAction()
    {
        var localVersion = "v"+Program.CurrentInstalledVersion;
        var remoteVersion = Releases.GetLatest();

        CanUpdate = localVersion != remoteVersion;
        UpdateText = $"Update ({localVersion} -> {remoteVersion})";
    }
    
    private void HideAllActions()
    {
        CanInstall = false;
        CanUninstall = false;
        CanUninstallBepInEx = false;
        CanUpdate = false;
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
    private bool _canInstall;
    private bool _canUninstall;
    private bool _canUninstallBepInEx;
    private bool _canUpdate;
    private string _installText = "Install";
}