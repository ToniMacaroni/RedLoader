using System;
using System.Diagnostics;
using System.Drawing;
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
            
            if(!LoaderEnvironment.IsDedicatedServer)
                RConsole.Init();
            GlobalKeyHook.Hook();

            ConfigSystem.Load();
            CorePreferences.Load();

            if (CorePreferences.AutoFixReshade.Value) 
                TryFixReshade();

            try
            {
                ReshadeManager.LoadUnity();
            }
            catch (Exception e)
            {
                Console.WriteLine("Reshade not found");
            }
            
            if(CorePreferences.ShowConsole.Value || LoaderEnvironment.IsDedicatedServer)
                RConsole.ShowConsole();

            if(!LoaderEnvironment.IsDedicatedServer)
                RConsole.SetConsoleRect(CorePreferences.ConsoleRect.Value);
            
            // If console is hidden force the splash window to show
            // On dedicated servers we don't want to show any gui including the splash window
            if (!LoaderEnvironment.IsDedicatedServer && (!CorePreferences.HideStatusWindow.Value || !CorePreferences.ShowConsole.Value))
            {
                //StatusWindow.Show();
                SplashWindow.CreateWindow();
                SplashWindow.TotalProgressSteps = 10;
                GlobalEvents.OnApplicationLateStart.Subscribe(SplashWindow.CloseWindow, 0, true);
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
            
            SplashWindow.PrintToConsole("Setting up utils...");
            SplashWindow.SetProgressSteps(1);
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

            SplashWindow.PrintToConsole("Loading config...");
            SplashWindow.SetProgressSteps(2);

            MelonCompatibilityLayer.LoadModules();

            bHapticsManager.Connect(BuildInfo.Name, UnityInformationHandler.GameName);

            SplashWindow.PrintToConsole("Loading Plugins...");
            SplashWindow.SetProgressSteps(3);
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
            SplashWindow.PrintToConsole("Setting up Il2Cpp...");
            SplashWindow.SetProgressSteps(4);
            if(!Directory.Exists(LoaderEnvironment.Il2CppAssembliesDirectory))
            {
                //RConsole.ShowConsole();
                SplashWindow.PrintToConsole("Generating Il2Cpp assemblies...");
            }

            SplashWindow.HookLog();
            var ret = Il2CppAssemblyGenerator.Run() ? 0 : 1;
            SplashWindow.UnhookLog();
            SplashWindow.PrintToConsole("Finished setting up Il2Cpp!");
            SplashWindow.SetProgressSteps(5);

            if (!CorePreferences.ShowConsole.Value && !LoaderEnvironment.IsDedicatedServer)
            {
                RConsole.HideConsole();
            }
            
            return ret;
        }

        internal static int Start()
        {
            if (GlobalKeyHook.DisableMods)
            {
                RLog.MsgDirect(Color.Orchid, new string('=', 50));
                RLog.MsgDirect(Color.Orchid, "Mods disabled due to [CONTROL KEY] being pressed during startup.");
                RLog.MsgDirect(Color.Orchid, new string('=', 50));
                SplashWindow.PrintToConsole("[error] Mods disabled due to [CONTROL KEY] being pressed during startup.");
                Thread.Sleep(2000);
                StatusWindow.CloseWindow();
                GlobalKeyHook.Unhook();
                return 0;
            }
            
            GlobalKeyHook.Unhook();

            GlobalEvents.OnPreModsLoaded.Invoke();
            
            SplashWindow.PrintToConsole("Loading Core Mods...");
            SplashWindow.SetProgressSteps(6);
            MelonHandler.LoadModsFromDirectory(LoaderEnvironment.CoreModDirectory, "Core mod");
            SplashWindow.PrintToConsole("Loading Mods...");
            SplashWindow.SetProgressSteps(7);
            MelonHandler.LoadModsFromDirectory(LoaderEnvironment.ModsDirectory, "Mod");

            GlobalEvents.OnPreSupportModule.Invoke();
            SplashWindow.PrintToConsole("Loading Support Modules...");
            SplashWindow.SetProgressSteps(8);
            if (!SupportModule.Setup())
                return 1;
            
            SplashWindow.PrintToConsole("Finishing up...");
            SplashWindow.SetProgressSteps(9);

            AddUnityDebugLog();
            RegisterTypeInIl2Cpp.SetReady();

            GlobalEvents.MelonHarmonyInit.Invoke();
            GlobalEvents.OnApplicationStart.Invoke();
            
            SplashWindow.PrintToConsole("* Ready! Loading Game...");
            SplashWindow.SetProgressSteps(10);
            
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

        private static void TryFixReshade()
        {
            var dir = Path.GetDirectoryName(Environment.ProcessPath);
            var dxgi = new FileInfo(Path.Combine(dir, "dxgi.dll"));
            if (dxgi.Exists)
            {
                dxgi.MoveTo(Path.Combine(dir, "Reshade64.dll"));
                RLog.Msg("Renamed dxgi.dll to Reshade64.dll . Set 'AutoFixReshade' to false in the config to stop this behavior.");
            }
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