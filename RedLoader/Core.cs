using System;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.IO;
using System.Runtime.InteropServices;
using bHapticsLib;
using RedLoader.InternalUtils;
using RedLoader.MonoInternals;
using RedLoader.Utils;

#if NET6_0
using System.Threading;
using RedLoader.CoreClrUtils;
#endif
#pragma warning disable IDE0051 // Prevent the IDE from complaining about private unreferenced methods

namespace RedLoader
{
	internal static class Core
    {
        internal static HarmonyLib.Harmony HarmonyInstance;
        
        internal static bool Is_ALPHA_PreRelease = false;

        internal static NativeLibrary.StringDelegate WineGetVersion;

        internal static int Initialize()
        {
            var runtimeFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var runtimeDirInfo = new DirectoryInfo(runtimeFolder);
            LoaderEnvironment.LoaderDirectory = runtimeDirInfo.Parent!.FullName;
            LoaderEnvironment.GameRootDirectory = Path.GetDirectoryName(LoaderEnvironment.GameExecutablePath);
            
            RConsole.Init();

            ConfigSystem.Load();
            CorePreferences.Load();
            
            if(CorePreferences.ShowConsole.Value || LoaderEnvironment.IsDedicatedServer)
                RConsole.ShowConsole();

            if(!LoaderEnvironment.IsDedicatedServer)
                RConsole.SetConsoleRect(CorePreferences.ConsoleRect.Value);
            
            // If console is hidden force the status window to show
            if (!LoaderEnvironment.IsDedicatedServer && (!CorePreferences.HideStatusWindow.Value || !CorePreferences.ShowConsole.Value))
            {
                StatusWindow.Show();
                GlobalEvents.OnApplicationLateStart.Subscribe(StatusWindow.CloseWindow, 0, true);
            }

            LaunchOptions.Load();

#if NET6_0
            if (LaunchOptions.Core.UserWantsDebugger && LoaderEnvironment.IsDotnetRuntime)
            {
                Console.WriteLine("[Init] User requested debugger, attempting to launch now...");
                Debugger.Launch();
            }
#endif

#if NET6_0
            Environment.SetEnvironmentVariable("IL2CPP_INTEROP_DATABASES_LOCATION", LoaderEnvironment.Il2CppAssembliesDirectory);
#endif
            
            SetupWineCheck();

            if (LoaderUtils.IsUnderWineOrSteamProton())
                Pastel.ConsoleExtensions.Disable();

            ManagedAnalyticsBlocker.Install();

            Fixes.DotnetLoadFromManagedFolderFix.Install();
            Fixes.UnhandledException.Install(AppDomain.CurrentDomain);
            Fixes.ServerCertificateValidation.Install();
            
            StatusWindow.StatusText = "Setting up utils...";
            LoaderUtils.Setup(AppDomain.CurrentDomain);

            Assertions.LemonAssertMapping.Setup();

            try
            {
                if (!MonoLibrary.Setup()
                    || !MonoResolveManager.Setup())
                    return 1;
            }
            catch (SecurityException)
            {
                MelonDebug.Msg("[MonoLibrary] Caught SecurityException, assuming not running under mono and continuing with init");
            }

            HarmonyInstance = new HarmonyLib.Harmony(BuildInfo.Name);
            
#if NET6_0
            // if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                // NativeStackWalk.LogNativeStackTrace();

            Fixes.DotnetAssemblyLoadContextFix.Install();
            Fixes.DotnetModHandlerRedirectionFix.Install();
#endif

            Fixes.ForcedCultureInfo.Install();
            Fixes.InstancePatchFix.Install();
            //Fixes.HarmonyExceptionFix.Install();
            Fixes.ProcessFix.Install();
            if(CorePreferences.ReadableExceptions.Value)
                Fixes.ExceptionFix.Install();
            PatchShield.Install();

            StatusWindow.StatusText = "Loadig config...";

            MelonCompatibilityLayer.LoadModules();

            bHapticsManager.Connect(BuildInfo.Name, UnityInformationHandler.GameName);

            StatusWindow.StatusText = "Loading Plugins...";
            MelonHandler.LoadMelonsFromDirectory<LoaderPlugin>(LoaderEnvironment.PluginsDirectory);
            GlobalEvents.MelonHarmonyEarlyInit.Invoke();
            GlobalEvents.OnPreInitialization.Invoke();

            return 0;
        }

        internal static int PreStart()
        {
            GlobalEvents.OnApplicationEarlyStart.Invoke();
            return MelonStartScreen.LoadAndRun(Il2CppGameSetup);
        }

        private static int Il2CppGameSetup()
        {
            StatusWindow.StatusText = "Setting up Il2Cpp...";
            if(!Directory.Exists(LoaderEnvironment.Il2CppAssembliesDirectory))
            {
                RConsole.ShowConsole();
                StatusWindow.StatusText = "Generating Il2Cpp assemblies...";
            }

            var ret = Il2CppAssemblyGenerator.Run() ? 0 : 1;
            StatusWindow.StatusText = "Finished setting up Il2Cpp!";

            if (!CorePreferences.ShowConsole.Value && !LoaderEnvironment.IsDedicatedServer)
            {
                RConsole.HideConsole();
            }
            
            return ret;
        }

        internal static int Start()
        {
            GlobalEvents.OnPreModsLoaded.Invoke();
            StatusWindow.StatusText = "Loading Core Mods...";
            MelonHandler.LoadModsFromDirectory(LoaderEnvironment.CoreModDirectory, "Core mod");
            StatusWindow.StatusText = "Loading Mods...";
            MelonHandler.LoadModsFromDirectory(LoaderEnvironment.ModsDirectory, "Mod");

            GlobalEvents.OnPreSupportModule.Invoke();
            StatusWindow.StatusText = "Loading Support Modules...";
            if (!SupportModule.Setup())
                return 1;
            
            StatusWindow.StatusText = "Finishing up...";

            AddUnityDebugLog();
            RegisterTypeInIl2Cpp.SetReady();

            GlobalEvents.MelonHarmonyInit.Invoke();
            GlobalEvents.OnApplicationStart.Invoke();
            
            StatusWindow.StatusText = "Loading V8 Engine...";
            
            StatusWindow.StatusText = "Ready! Loading Game...";

            return 0;
        }
        
        internal static string GetVersionString()
        {
            var versionStr = $"RedLoader " +
                             $"v{BuildInfo.Version} " +
                             $"{(Is_ALPHA_PreRelease ? "ALPHA Pre-Release" : "Open-Beta")}";
            return versionStr;
        }
        
        internal static void WelcomeMessage()
        {
            //if (MelonDebug.IsEnabled())
            //    MelonLogger.WriteSpacer();

            RLog.MsgDirect("------------------------------");
            RLog.MsgDirect(GetVersionString());
            RLog.MsgDirect($"OS: {GetOSVersion()}");
            RLog.MsgDirect($"Hash Code: {LoaderUtils.HashCode}");
            RLog.MsgDirect("------------------------------");

            LoaderEnvironment.PrintEnvironment();
        }

        [DllImport("ntdll.dll", SetLastError = true)]
        internal static extern uint RtlGetVersion(out OsVersionInfo versionInformation); // return type should be the NtStatus enum
        
        [StructLayout(LayoutKind.Sequential)]
        internal struct OsVersionInfo
        {
            private readonly uint OsVersionInfoSize;

            internal readonly uint MajorVersion;
            internal readonly uint MinorVersion;

            internal readonly uint BuildNumber;

            private readonly uint PlatformId;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            internal readonly string CSDVersion;
        }
        
        internal static string GetOSVersion()
        {
            if (LoaderUtils.IsUnix || LoaderUtils.IsMac)
                return Environment.OSVersion.VersionString;
            
            if (LoaderUtils.IsUnderWineOrSteamProton())
                return $"Wine {WineGetVersion()}";
            RtlGetVersion(out OsVersionInfo versionInformation);
            var minor = versionInformation.MinorVersion;
            var build = versionInformation.BuildNumber;

            string versionString = "";

            switch (versionInformation.MajorVersion)
            {
                case 4:
                    versionString = "Windows 95/98/Me/NT";
                    break;
                case 5:
                    if (minor == 0)
                        versionString = "Windows 2000";
                    if (minor == 1)
                        versionString = "Windows XP";
                    if (minor == 2)
                        versionString = "Windows 2003";
                    break;
                case 6:
                    if (minor == 0)
                        versionString = "Windows Vista";
                    if (minor == 1)
                        versionString = "Windows 7";
                    if (minor == 2)
                        versionString = "Windows 8";
                    if (minor == 3)
                        versionString = "Windows 8.1";
                    break;
                case 10:
                    if (build >= 22000)
                        versionString = "Windows 11";
                    else
                        versionString = "Windows 10";
                    break;
                default:
                    versionString = "Unknown";
                    break;
            }

            return $"{versionString}";
        }
        
        internal static void Quit()
        {
            MelonDebug.Msg("[ML Core] Received Quit from Support Module. Shutting down...");
            
            if(CorePreferences.SaveConsoleRect.Value)
                RConsole.SaveConsoleRect();
            
            ConfigSystem.Save();

            HarmonyInstance.UnpatchSelf();
            bHapticsManager.Disconnect();

            RLog.Flush();
            //MelonLogger.Close();
            
            System.Threading.Thread.Sleep(200);

            if (LaunchOptions.Core.QuitFix)
                Process.GetCurrentProcess().Kill();
        }
        
        private static void SetupWineCheck()
        {
            if (LoaderUtils.IsUnix || LoaderUtils.IsMac)
                return;
            
            IntPtr dll = NativeLibrary.LoadLib("ntdll.dll");
            IntPtr wine_get_version_proc = NativeLibrary.AgnosticGetProcAddress(dll, "wine_get_version");
            if (wine_get_version_proc == IntPtr.Zero)
                return;

            WineGetVersion = (NativeLibrary.StringDelegate)Marshal.GetDelegateForFunctionPointer(
                wine_get_version_proc,
                typeof(NativeLibrary.StringDelegate)
            );
        }

        private static void AddUnityDebugLog()
        {
            var msg = "~   This Game has been MODIFIED using RedLoader. DO NOT report any issues to the Game Developers!   ~";
            var line = new string('-', msg.Length);
            SupportModule.Interface.UnityDebugLog(line);
            SupportModule.Interface.UnityDebugLog(msg);
            SupportModule.Interface.UnityDebugLog(line);
        }
    }
}