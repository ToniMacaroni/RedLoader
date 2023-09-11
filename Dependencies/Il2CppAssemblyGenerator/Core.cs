using System.IO;
using System.Net;
using System.Net.Http;
using RedLoader.Il2CppAssemblyGenerator.Packages;
using RedLoader.Modules;
using RedLoader.Utils;

namespace RedLoader.Il2CppAssemblyGenerator
{
    internal class Core : MelonModule
    {
        internal static string BasePath = null;
        internal static string GameAssemblyPath = null;
        //internal static string ManagedPath = null;

        internal static HttpClient webClient = null;

        internal static Packages.Models.ExecutablePackage dumper = null;
        internal static Packages.Il2CppInterop il2cppinterop = null;
        internal static UnityDependencies unitydependencies = null;
        internal static DeobfuscationMap deobfuscationMap = null;
        internal static DeobfuscationRegex deobfuscationRegex = null;

        internal static bool AssemblyGenerationNeeded = false;

        internal static RLog.Instance Logger;

        public override void OnInitialize()
        {
            Logger = LoggerInstance;

            webClient = new();
            webClient.DefaultRequestHeaders.Add("User-Agent", $"{BuildInfo.Name} v{BuildInfo.Version}");

            AssemblyGenerationNeeded = LaunchOptions.Il2CppAssemblyGenerator.ForceRegeneration;

            string gameAssemblyName = "GameAssembly";
            
            if (LoaderUtils.IsUnix)
                gameAssemblyName += ".so"; 
            if (LoaderUtils.IsWindows)
                gameAssemblyName += ".dll";
            if (LoaderUtils.IsMac)
                gameAssemblyName += ".dylib";

            GameAssemblyPath = Path.Combine(LoaderEnvironment.GameRootDirectory, gameAssemblyName);
            //ManagedPath = MelonEnvironment.MelonManagedDirectory;

            BasePath = Path.GetDirectoryName(Assembly.Location);
        }

        private static int Run()
        {
            Config.Initialize();

            string CurrentGameAssemblyHash;
            Logger.Msg("Checking GameAssembly...");
            MelonDebug.Msg($"Last GameAssembly Hash: {Config.Values.GameAssemblyHash}");
            MelonDebug.Msg($"Current GameAssembly Hash: {CurrentGameAssemblyHash = FileHandler.Hash(GameAssemblyPath)}");

            if (string.IsNullOrEmpty(Config.Values.GameAssemblyHash)
                || !Config.Values.GameAssemblyHash.Equals(CurrentGameAssemblyHash))
                AssemblyGenerationNeeded = true;

            if (!AssemblyGenerationNeeded)
            {
                Logger.Msg("Assembly is up to date. No Generation Needed.");
                return 0;
            }
            
            Logger.Msg("Assembly Generation Needed!");
            
            //if (!LaunchOptions.Il2CppAssemblyGenerator.OfflineMode)
                RemoteAPI.Contact();

            dumper = new Cpp2IL();
            il2cppinterop = new Packages.Il2CppInterop();
            unitydependencies = new UnityDependencies();
            deobfuscationMap = new DeobfuscationMap();
            deobfuscationRegex = new DeobfuscationRegex();

            Logger.Msg($"Using Dumper Version: {(string.IsNullOrEmpty(dumper.Version) ? "null" : dumper.Version)}");
            Logger.Msg($"Using Il2CppInterop Version = {(string.IsNullOrEmpty(il2cppinterop.Version) ? "null" : il2cppinterop.Version)}");
            Logger.Msg($"Using Unity Dependencies Version = {(string.IsNullOrEmpty(unitydependencies.Version) ? "null" : unitydependencies.Version)}");
            Logger.Msg($"Using Deobfuscation Regex = {(string.IsNullOrEmpty(deobfuscationRegex.Regex) ? "null" : deobfuscationRegex.Regex)}");

            if (!dumper.Setup()
                || !il2cppinterop.Setup()
                || !unitydependencies.Setup()
                || !deobfuscationMap.Setup())
                return 1;

            deobfuscationRegex.Setup();

            dumper.Cleanup();
            il2cppinterop.Cleanup();

            if (!dumper.Execute())
            {
                dumper.Cleanup();
                return 1;
            }

            if (!il2cppinterop.Execute())
            {
                dumper.Cleanup();
                il2cppinterop.Cleanup();
                return 1;
            }

            OldFiles_Cleanup();
            OldFiles_LAM();

            dumper.Cleanup();
            il2cppinterop.Cleanup();

            Logger.Msg("Assembly Generation Successful!");
            deobfuscationRegex.Save();
            Config.Values.GameAssemblyHash = CurrentGameAssemblyHash;
            Config.Save();

            return 0;
        }

        private static void OldFiles_Cleanup()
        {
            if (Config.Values.OldFiles.Count <= 0)
                return;
            for (int i = 0; i < Config.Values.OldFiles.Count; i++)
            {
                string filename = Config.Values.OldFiles[i];
                string filepath = Path.Combine(LoaderEnvironment.Il2CppAssembliesDirectory, filename);
                if (File.Exists(filepath))
                {
                    Logger.Msg("Deleting " + filename);
                    File.Delete(filepath);
                }
            }
            Config.Values.OldFiles.Clear();
        }

        private static void OldFiles_LAM()
        {
            string[] filepathtbl = Directory.GetFiles(il2cppinterop.OutputFolder);
            string il2CppAssembliesDirectory = LoaderEnvironment.Il2CppAssembliesDirectory;
            Directory.CreateDirectory(il2CppAssembliesDirectory);
            
            for (int i = 0; i < filepathtbl.Length; i++)
            {
                string filepath = filepathtbl[i];
                string filename = Path.GetFileName(filepath);
                Logger.Msg("Moving " + filename);
                Config.Values.OldFiles.Add(filename);
                string newfilepath = Path.Combine(il2CppAssembliesDirectory, filename);
                if (File.Exists(newfilepath))
                    File.Delete(newfilepath);
                File.Move(filepath, newfilepath);
                
                //NamespacePrefixer.TryProcess(newfilepath);
            }
            
            Config.Save();
        }
    }
}