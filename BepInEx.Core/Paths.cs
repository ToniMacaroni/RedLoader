using System.IO;
using System.Linq;
using System.Reflection;
//using MonoMod.Utils;
//using SemanticVersioning;

namespace BepInEx;

/// <summary>
///     Paths used by BepInEx
/// </summary>
public static class Paths
{
    // TODO: Why is this in Paths?
    /// <summary>
    ///    BepInEx version.
    /// </summary>
    // public static Version BepInExVersion { get; } =
    //     Version.Parse(MetadataHelper.GetAttributes<AssemblyInformationalVersionAttribute>(typeof(Paths).Assembly)[0]
    //                                 .InformationalVersion);

    /// <summary>
    ///     The path to the Managed folder that contains the main managed assemblies.
    /// </summary>
    public static string ManagedPath { get; private set; }

    /// <summary>
    ///     The path to the game data folder of the currently running Unity game.
    /// </summary>
    public static string GameDataPath { get; private set; }

    /// <summary>
    ///     The directory that the core BepInEx DLLs reside in.
    /// </summary>
    public static string BepInExAssemblyDirectory { get; private set; }

    /// <summary>
    ///     The path to the core BepInEx DLL.
    /// </summary>
    public static string BepInExAssemblyPath { get; private set; }

    /// <summary>
    ///     The path to the main BepInEx folder.
    /// </summary>
    public static string BepInExRootPath { get; private set; }

    /// <summary>
    ///     The path of the currently executing program BepInEx is encapsulated in.
    /// </summary>
    public static string ExecutablePath { get; private set; }

    /// <summary>
    ///     The directory that the currently executing process resides in.
    ///     <para>On OSX however, this is the parent directory of the game.app folder.</para>
    /// </summary>
    public static string GameRootPath { get; private set; }

    /// <summary>
    ///     The path to the config directory.
    /// </summary>
    public static string ConfigPath { get; private set; }

    /// <summary>
    ///     The path to the global BepInEx configuration file.
    /// </summary>
    public static string BepInExConfigPath { get; private set; }

    /// <summary>
    ///     The path to temporary cache files.
    /// </summary>
    public static string CachePath { get; private set; }

    /// <summary>
    ///     The path to the patcher plugin folder which resides in the BepInEx folder.
    /// </summary>
    public static string PatcherPluginPath { get; private set; }

    /// <summary>
    ///     The path to the plugin folder which resides in the BepInEx folder.
    ///     <para>
    ///         This is ONLY guaranteed to be set correctly when Chainloader has been initialized.
    ///     </para>
    /// </summary>
    public static string PluginPath { get; private set; }

    /// <summary>
    ///     The name of the currently executing process.
    /// </summary>
    public static string ProcessName { get; private set; }

    /// <summary>
    ///     List of directories from where Mono will search assemblies before assembly resolving is invoked.
    /// </summary>
    public static string[] DllSearchPaths { get; private set; }

    public static void SetExecutablePath(string executablePath)
    {
        ExecutablePath = executablePath;
        ProcessName = Path.GetFileNameWithoutExtension(executablePath);

        GameRootPath = Path.GetDirectoryName(executablePath);

        GameDataPath = Path.Combine(GameRootPath, $"{ProcessName}_Data");
        ManagedPath = Path.Combine(GameDataPath, "Managed");
        BepInExRootPath = Path.Combine(GameRootPath, "SFLoader");
        ConfigPath = Path.Combine(GameRootPath, "UserData");
        BepInExConfigPath = Path.Combine(ConfigPath, "BepInEx.cfg");
        PluginPath = Path.Combine(GameRootPath, "Mods");
        PatcherPluginPath = Path.Combine(GameRootPath, "Plugins");
        BepInExAssemblyDirectory = Path.Combine(BepInExRootPath, "net6");
        BepInExAssemblyPath = Path.Combine(BepInExAssemblyDirectory,
                                           "SFLoader.dll");
        CachePath = Path.Combine(BepInExRootPath, "cache");
        DllSearchPaths = new[] { ManagedPath };
    }

    // public static void SetPluginPath(string pluginPath) =>
    //     PluginPath = Utility.CombinePaths(BepInExRootPath, pluginPath);
}