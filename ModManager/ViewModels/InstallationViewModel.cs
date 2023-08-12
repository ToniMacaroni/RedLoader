using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using MelonLoader;
using Microsoft.Win32;
using SFLoader;

namespace ModManager.ViewModels;

public class InstallationViewModel : INotifyPropertyChanged
{
    public static InstallationViewModel Instance { get; private set; } = new();

    public ICommand InstallCommand { get; set; }
    public ICommand InstallUeCommand { get; set; }
    
    public ICommand UninstallCommand { get; set; }
    public ICommand UninstallBepInExCommand { get; set; }
    public ICommand UninstallMelonCommand { get; set; }
    
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

    public bool PathIsValid
    {
        get => _pathIsValid;
        set
        {
            _pathIsValid = value;
            OnPropertyChanged();
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
            _canInstall = value;
            OnPropertyChanged();
        }
    }

    public bool CanUninstall
    {
        get => _canUninstall;
        set
        {
            _canUninstall = value;
            OnPropertyChanged();
        }
    }

    public bool CanUninstallBepInEx
    {
        get => _canUninstallBepInEx;
        set
        {
            _canUninstallBepInEx = value;
            OnPropertyChanged();
        }
    }
    
    public bool CanUninstallMelon
    {
        get => _canUninstallBepInEx;
        set
        {
            _canUninstallBepInEx = value;
            OnPropertyChanged();
        }
    }

    public bool CanUpdate
    {
        get => _canUpdate;
        set
        {
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

    public string InstallUeText
    {
        get => _installUeText;
        set
        {
            _installUeText = value;
            OnPropertyChanged();
        }
    }

    public bool CanInstallUe
    {
        get => _canInstallUe;
        set
        {
            _canInstallUe = value;
            OnPropertyChanged();
        }
    }

    public Visibility ProgressVisibility => CurrentProgress > 0 ? Visibility.Visible : Visibility.Collapsed;

    public InstallationViewModel()
    {
        InstallCommand = new RelayCommand(Install);
        InstallUeCommand = new RelayCommand(InstallUe);
        
        UninstallCommand = new RelayCommand(ClearSfLoader);
        UninstallBepInExCommand = new RelayCommand(ClearBepInEx);
        UninstallMelonCommand = new RelayCommand(ClearMelon);
        
        UpdateCommand = new RelayCommand(Update);
        BrowseCommand = new RelayCommand(Browse);
        InstallPath = PathTools.GetGamePath() ?? "Select SonsOfTheForest.exe";
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void Install(object obj)
    {
        if (!ShowInsallationDialog())
            return;
        
        CleanerDatabase.BieCleaner.Clear();
        CleanerDatabase.MelonCleaner.Clear();
        
        HideAllActions();
#if DEBUG
        var installer = new DebugInstaller(@"I:\repos\MelonLoader\SFLoader.zip", "SFLoader", CleanerDatabase.PartialSfLoaderCleaner);
        installer.Install();
#else
        var installer = new GithubInstaller(GithubInfoDatabase.SFLoader, "SFLoader", CleanerDatabase.PartialSfLoaderCleaner);
        installer.Install();
#endif
    }
    
    private void InstallUe(object obj)
    {
        if (!PathIsValid)
            return;

        CanInstallUe = false;
        InstallUeText = "Installing...";
        var installer = new GithubInstaller(GithubInfoDatabase.UnityExplorer, "UnityExplorer", CleanerDatabase.UeCleaner);
        installer.Install();
    }

    private void Clear(object obj)
    {
        HideAllActions();
        CleanerDatabase.PartialSfLoaderCleaner.Clear();
    }

    private void ClearBepInEx(object obj)
    {
        CleanInstallation(CleanerDatabase.BieCleaner);
    }
    
    private void ClearMelon(object obj)
    {
        CleanInstallation(CleanerDatabase.MelonCleaner);
    }

    private void ClearSfLoader(object obj)
    {
        CleanInstallation(CleanerDatabase.PartialSfLoaderCleaner);
    }
    
    private bool CleanInstallation(InstallationCleaner cleaner)
    {
        if (!ShowRemovalDialog(cleaner))
        {
            return false;
        }
        
        cleaner.Clear();

        RefreshActionAvailability();

        CurrentState = $"{cleaner.Name} uninstalled";
        return true;
    }

    private void Update(object obj)
    {
        Install(obj);
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
            PathIsValid = false;
            CanInstall = false;
            
            CanUninstall = false;
            CanUninstallBepInEx = false;
            CanUninstallMelon = false;
            
            CanUpdate = false;
            CurrentState = "Select game executable...";
            
            RefreshAdditionalActions();
            return;
        }

        CurrentState = "Select action...";
        PathIsValid = true;
        
        RefreshAdditionalActions();

        Program.GetCurrentInstallVersion(Path.GetDirectoryName(InstallPath));
        var localVersion = Program.GetFileVersion(Path.Combine(GetDirectoryPath(), "_SFLoader", "net6", "SFLoader.dll"));
        
        if (localVersion == null)
        {
            // Install
            CanInstall = true;
            CanUninstall = false;
            CanUpdate = false;
            InstallText = $"Install ({GithubInfoDatabase.SFLoader.GetLatest()})";
            RefreshUninstallerActions();
            return;
        }

        // SFLoader exists: Uninstall or Update
        CanInstall = false;
        CanUninstall = true;
        RefreshUpdateAction();
        RefreshUninstallerActions();
    }

    public static void Refresh()
    {
        if (Instance == null)
        {
            return;
        }

        Instance.RefreshActionAvailability();
    }

    private void RefreshAdditionalActions()
    {
        CanInstallUe = PathIsValid;
        InstallUeText = CleanerDatabase.UeCleaner.IsInstalled() ? "Update UnityExplorer" : "Install UnityExplorer";
    }

    private void RefreshUninstallerActions()
    {
        RefreshBepInExActionAvailability();
        RefreshMelonActionAvailability();
    }

    private void RefreshBepInExActionAvailability()
    {
        CanUninstallBepInEx = CleanerDatabase.BieCleaner.IsInstalled();
    }
    
    private void RefreshMelonActionAvailability()
    {
        CanUninstallMelon = CleanerDatabase.MelonCleaner.IsInstalled();
    }

    private void RefreshUpdateAction()
    {
        var localVersion = "v"+Program.CurrentInstalledVersion;
        var remoteVersion = GithubInfoDatabase.SFLoader.GetLatest();

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

    private bool ShowRemovalDialog(InstallationCleaner cleaner)
    {
        var messageResult =
            MessageBox.Show(
                $"This will completely remove {cleaner.Name}. Make sure to backup your mods to another location if you want to keep them. Continue?",
                "Warning", MessageBoxButton.YesNo);
        return messageResult == MessageBoxResult.Yes;
    }
    
    private bool ShowInsallationDialog()
    {
        var messageResult =
            MessageBox.Show(
                $"This will completely remove any previous loader installation other than SFLoader. Make sure to backup your mods to another location if you want to keep them. Continue?",
                "Warning", MessageBoxButton.YesNo);
        return messageResult == MessageBoxResult.Yes;
    }

    private bool IsPathValid()
    {
        return InstallPath.EndsWith("SonsOfTheForest.exe") && File.Exists(InstallPath);
    }

    public static string GetDirectoryPath()
    {
        return Path.GetDirectoryName(Instance.InstallPath);
    }
    
    public static string GetInstallPath()
    {
        return Instance.InstallPath;
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
        Instance.CurrentProgress = 0;
    }

    private string _installPath = "Select SonsOfTheForest.exe";
    private int _currentProgress = 0;
    private string _currentState = "Select the game executable!";
    private int _maxProgress = 100;
    private string _updateText = "Update";
    private bool _canInstall;
    private bool _canUninstall;
    private bool _canUninstallBepInEx;
    private bool _canUpdate;
    private string _installText = "Install";
    private string _installUeText = "Install UE";
    private bool _pathIsValid;
    private bool _canInstallUe;
}