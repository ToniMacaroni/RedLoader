using System.Drawing;
using System.Reflection;
using HarmonyLib;
using RedLoader;
using RedLoader.Bootstrap;
using RedLoader.Utils;
using Sons.Events;
using Sons.Gameplay.GameSetup;
using Sons.Multiplayer;
using Sons.Settings;
using SonsGameManager;
using SonsLoaderPlugin;
using SonsSdk.Attributes;
using SonsSdk.Networking;
using SonsSdk.Private;
using Steamworks;
using SUI;
using TheForest;
using TheForest.Utils;
using Version = System.Version;

namespace SonsSdk;

public class SdkEntryPoint : IModProcessor
{
    internal static HarmonyLib.Harmony Harmony = new("sonssdk_harmony");
    
    public List<ModBase> LoadPlugins()
    {
        var result = new List<ModBase>();
        
        foreach (var pluginPath in Directory.EnumerateFiles(LoaderEnvironment.ModsDirectory, "*.dll"))
        {
            result.AddRange(TryLoadFromPath(pluginPath));
        }

        return result;
    }
    
    public List<ModBase> TryLoadFromPath(string assemblyPath)
    {
        var mods = new List<ModBase>();

        var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
        var manifestPath = Path.Combine(LoaderEnvironment.ModsDirectory, assemblyName, "manifest.json");
        var manifest = ManifestReader.TryReadManifest(manifestPath);
        if (manifest != null)
        {
            if (manifest.Platform == "Client" && LoaderEnvironment.IsDedicatedServer)
            {
                RLog.Error($"Mod {assemblyName} is a client mod and cannot be loaded on a dedicated server.");
                SonsMod.ReportMod(manifest.Id, "Client mod cannot be loaded on a dedicated server.");
            }
            else if (manifest.Platform == "Server" && !LoaderEnvironment.IsDedicatedServer)
            {
                RLog.Error($"Mod {assemblyName} is a server mod and cannot be loaded on a client.");
                SonsMod.ReportMod(manifest.Id, "Server mod cannot be loaded on a client.");
            }
            // TODO: Check Compatibility
            else if (!string.IsNullOrEmpty(manifest.LoaderVersion) && !LoaderUtils.IsCompatible(manifest.LoaderVersion))
            {
                RLog.Error($"Mod {assemblyName} requires a different version of RedLoader.");
                SonsMod.ReportMod(manifest.Id, $"Requires RedLoader >={manifest.LoaderVersion}");
            }
            else if (manifest.Type == ManifestData.EAssemblyType.Library)
            {
                RLog.Error($"{assemblyName} is a library but was put into the mods folder.");
                SonsMod.ReportMod(manifest.Id, $"Is a library but was put into the mods folder.");
            }
            else if (InitMod(assemblyPath, manifest, out var mod))
            {
                if (!string.IsNullOrEmpty(manifest.GameVersion) && !CheckGameVersionCompatibility(manifest.GameVersion))
                {
                    RLog.Warning($"{assemblyName} is made for a different version of the game.");
                    RLog.Warning($"The mod will still load, but may not work correctly.");
                    SonsMod.ReportMod(manifest.Id, "Mod was loaded, but was made for a different game version");
                }
                
                RLog.Msg(System.ConsoleColor.Magenta, $"Loaded mod {mod.Manifest.Name} for game {manifest.GameVersion}");
                
                mods.Add(mod);

                mod.AssetBundleAttrs = AssetBundleAttributeLoader.GetAllTypes(mod);
                foreach (var attr in mod.AssetBundleAttrs)
                {
                    attr.Mod = mod;
                }
            }
        }
        else
        {
            RLog.Error($"{assemblyName} does not have a manifest.json file.");
            SonsMod.ReportMod(assemblyName, "Missing manifest.json");
        }

        return mods;
    }

    private bool CheckGameVersionCompatibility(string version)
    {
        if (version.Contains('.'))
            return true;
        
        return version == Sons.GameApplication.Version.GetVersionString();
    }
    
    private bool InitMod(string assemblyPath, ManifestData data, out SonsMod outMod)
    {
        outMod = null;
        
        try
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
        
            var implementation = assembly.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(SonsMod)));
            if (implementation == null)
            {
                RLog.Error($"Failed to find a valid implementation of SonsMod in {assembly.FullName}");
                return false;
            }
            
            outMod = (SonsMod)Activator.CreateInstance(implementation, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, null, null);
            
            if(outMod == null)
            {
                RLog.Error($"Failed to create an instance of {implementation.FullName}");
                return false;
            }
            
            var version = new Version(data.Version);
            data.VersionObject = version;
                
            outMod.Info = new MelonInfoAttribute(
                                                 outMod.GetType(), 
                                                 string.IsNullOrEmpty(data.Name) ? data.Id : data.Name, 
                                                 version.Major, 
                                                 version.Minor,
                                                 version.Build,
                                                 data.Author,
                                                 data.Url);

            outMod.Manifest = data;
                
            outMod.ModAssembly = assembly;
            outMod.Priority = data.Priority;
            outMod.ConsoleColor = LoaderUtils.ColorFromString(data.LogColor);
            outMod.AuthorConsoleColor = RLog.DefaultTextColor;
            outMod.SupportedGameVersion = data.GameVersion;
            outMod.OptionalDependencies = data.Dependencies;
            outMod.ID = data.Id;

            outMod.Register();
            
            return true;
        }
        catch (Exception e)
        {
            RLog.Error(e);
            return false;
        }
    }

    public void InitAfterUnity()
    {
        GlobalEvents.OnApplicationLateStart.Subscribe(OnAppLateStart);
        SdkEvents.OnSdkInitialized.Subscribe(OnSdkInitialized);
        SdkEvents.OnGameStart.Subscribe(OnGameStart);
        SdkEvents.OnCheatsEnabledChanged.Subscribe(OnCheatsEnabledChanged);
        MiscPatches.Init();
    }

    private void OnAppLateStart()
    {
        GlobalOverlays.CreateBlackScreen();
        SdkEvents.Init();
    }

    private void OnSdkInitialized()
    {
        ModManagerUi.Create();
    }

    private void OnGameStart()
    {
        GameCommands.Init();

        if (!BoltNetwork.isRunning) 
            SetConsoleEnabled(true);
        
        PanelBlur.SetupBlur();
    }

    private void SetConsoleEnabled(bool shouldEnable)
    {
        DebugConsole.SetCheatsAllowed(shouldEnable);
        DebugConsole.Instance.SetBlockConsole(!shouldEnable);
    }

    private bool IsPowerPlayer()
    {
        if (!BoltNetwork.isRunning)
            return true;

        if (BoltNetwork.isServer)
            return true;
        
        return NetUtils.HasUserPermission(SonsTools.GetSteamId(), PlayerRoles.Owner);
    }
    
    private void OnCheatsEnabledChanged(bool isEnabled)
    {
        SetConsoleEnabled(isEnabled);
        if (LocalPlayer._instance)
        {
            SonsTools.ShowMessage("Cheats have been " + (isEnabled ? "enabled" : "disabled"));
        }
    }
}


