using System;
using System.Drawing;
using System.IO;
using System.Reflection;

#if !NET6_0
using System.Diagnostics;
using RedLoader;
#endif

namespace RedLoader.Utils
{
    public static class LoaderEnvironment
    {
        private const string OurRuntimeName =
#if !NET6_0
            "net35";
#else
            "net6";
#endif

        public static bool IsDotnetRuntime { get; } = OurRuntimeName == "net6";
        public static bool IsMonoRuntime { get; } = !IsDotnetRuntime;
        
        public static bool IsDedicatedServer { get; internal set; }

        /// <summary>
        /// Path of {gameroot}/{loader}/
        /// </summary>
        public static string LoaderDirectory { get; internal set; }
        
        /// <summary>
        /// Path of the directory where the game executable is located
        /// </summary>
        public static string GameRootDirectory { get; internal set; }

#if NET6_0
        public static string GameExecutablePath => System.Environment.ProcessPath;
#else
        public static string GameExecutablePath => Process.GetCurrentProcess().MainModule!.FileName;
#endif
        public static string DependenciesDirectory => Path.Combine(LoaderDirectory, "Dependencies");
        public static string SupportModuleDirectory => Path.Combine(DependenciesDirectory, "SupportModules");
        public static string CompatibilityLayerDirectory => Path.Combine(DependenciesDirectory, "CompatibilityLayers");
        public static string Il2CppAssemblyGeneratorDirectory => Path.Combine(DependenciesDirectory, "Il2CppAssemblyGenerator");
        public static string ModsDirectory => Path.Combine(GameRootDirectory, "Mods");
        public static string PluginsDirectory => Path.Combine(LoaderDirectory, "Plugins");
        public static string LibsDirectory => Path.Combine(GameRootDirectory, "Libs");
        public static string UserDataDirectory => Path.Combine(GameRootDirectory, "UserData");
        public static string OurRuntimeDirectory => Path.Combine(LoaderDirectory, OurRuntimeName);

        /// <summary>
        /// Name of the executable without the extension
        /// </summary>
        public static string GameExecutableName => Path.GetFileNameWithoutExtension(GameExecutablePath);
        public static string UnityGameDataDirectory => Path.Combine(GameRootDirectory, GameExecutableName + "_Data");
        public static string Il2CppDataDirectory => Path.Combine(UnityGameDataDirectory, "il2cpp_data");
        public static string UnityPlayerPath => Path.Combine(GameRootDirectory, "UnityPlayer.dll");
        
        /// <summary>
        /// Directory of the core mods
        /// </summary>
        internal static string CoreModDirectory => Path.Combine(LoaderDirectory, "CoreMods");
        
        public static string PendingDirectory => Path.Combine(LoaderDirectory, "Pending");

        //public static string MelonManagedDirectory => Path.Combine(LoaderDirectory, "Managed");
        public static string Il2CppAssembliesDirectory => Path.Combine(LoaderDirectory, "Game");
        
        /// <summary>
        /// The folder name of the loader
        /// </summary>
        public static string LoaderFolderName => Path.GetFileName(LoaderDirectory);

        internal static void PrintEnvironment()
        {
            if (IsDedicatedServer)
            {
                void DedPrint(string text)
                {
                    RLog.MsgDirect(Color.PaleVioletRed, text);
                }
                
                PrettyPrint.Print(new []{"RED FOR SERVER"}, DedPrint, 2, 30);
            }
            
            //These must not be changed, lum needs them
            RLog.MsgDirect($"Core::BasePath = {GameRootDirectory}");
            RLog.MsgDirect($"Game::BasePath = {GameRootDirectory}");
            RLog.MsgDirect($"Game::DataPath = {UnityGameDataDirectory}");
            RLog.MsgDirect($"Game::ApplicationPath = {GameExecutablePath}");

            RLog.MsgDirect($"Runtime Type: {OurRuntimeName}");
        }

        static LoaderEnvironment()
        {
            IsDedicatedServer = Environment.ProcessPath!.EndsWith("DS.exe");
        }

        public static PathObject GetModDataPath(Assembly assembly)
        {
            return new PathObject(Path.Combine(Path.GetDirectoryName(assembly.Location)!, assembly.GetName().Name!));
            // return Path.Combine(ModsDirectory, assembly.GetName().Name!);
        }
        
        public static PathObject GetModDataPath(ModBase mod)
        {
            return GetModDataPath(mod.ModAssembly.Assembly);
        }
        
        public static PathObject GetMetadataPath(Assembly assembly)
        {
            return GetModDataPath(assembly) / "manifest.json";
        }
    }
}