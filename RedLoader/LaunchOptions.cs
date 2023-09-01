using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace RedLoader
{
    public static class LaunchOptions
    {
        private static Dictionary<string, Action> WithoutArg = new Dictionary<string, Action>();
        private static Dictionary<string, Action<string>> WithArg = new Dictionary<string, Action<string>>();

        static LaunchOptions()
        {
            AnalyticsBlocker.Setup();
            Core.Setup();
            SonsSdk.Setup();
            Console.Setup();
            Il2CppAssemblyGenerator.Setup();
            Logger.Setup();
        }

        internal static void Load()
        {
            List<string> foundOptions = new List<string>();

            LemonEnumerator<string> argEnumerator = new LemonEnumerator<string>(Environment.GetCommandLineArgs());
            while (argEnumerator.MoveNext())
            {
                string fullcmd = argEnumerator.Current;
                if (string.IsNullOrEmpty(fullcmd))
                    continue;

                if (!fullcmd.StartsWith("--"))
                    continue;

                string cmd = fullcmd.Remove(0, 2);

                if (WithoutArg.TryGetValue(cmd, out Action withoutArgFunc))
                {
                    foundOptions.Add(fullcmd);
                    withoutArgFunc();
                }
                else if (WithArg.TryGetValue(cmd, out Action<string> withArgFunc))
                {
                    if (!argEnumerator.MoveNext())
                        continue;

                    string cmdArg = argEnumerator.Current;
                    if (string.IsNullOrEmpty(cmdArg))
                        continue;

                    if (cmdArg.StartsWith("--"))
                        continue;

                    foundOptions.Add($"{fullcmd} = {cmdArg}");
                    withArgFunc(cmdArg);
                }
            }

            if (foundOptions.Count <= 0)
                return;
        }

#region Args
        public static class AnalyticsBlocker
        {
            public static bool ShouldDAB { get; internal set; }

            internal static void Setup()
            {
                WithoutArg["dab"] = () => ShouldDAB = true;

            }
        }

        public static class Core
        {
            public enum LoadModeEnum
            {
                NORMAL,
                DEV,
                BOTH
            }
            public static LoadModeEnum LoadMode_Plugins { get; internal set; }
            public static LoadModeEnum LoadMode_Mods { get; internal set; }
            public static bool QuitFix { get; internal set; }
            public static bool StartScreen { get; internal set; } = true;
            public static string UnityVersion { get; internal set; }
            public static bool IsDebug { get; internal set; }
            public static bool UserWantsDebugger { get; internal set; }
            public static bool ShouldDisplayAnalyticsBlocker { get; internal set; }

            internal static void Setup()
            {
                WithoutArg["quitfix"] = () => QuitFix = true;
                WithoutArg["disablestartscreen"] = () => StartScreen = false;
                WithArg["loadmodeplugins"] = (string arg) =>
                {
                    if (int.TryParse(arg, out int valueint))
                        LoadMode_Plugins = (LoadModeEnum)LoaderUtils.Clamp(valueint, (int)LoadModeEnum.NORMAL, (int)LoadModeEnum.BOTH);
                };
                WithArg["loadmodemods"] = (string arg) =>
                {
                    if (int.TryParse(arg, out int valueint))
                        LoadMode_Mods = (LoadModeEnum)LoaderUtils.Clamp(valueint, (int)LoadModeEnum.NORMAL, (int)LoadModeEnum.BOTH);
                };
                WithArg["unityversion"] = (string arg) => UnityVersion = arg;
                WithoutArg["debug"] = () => IsDebug = true;
                WithoutArg["launchdebugger"] = () => UserWantsDebugger = true;
                WithoutArg["dab"] = () => ShouldDisplayAnalyticsBlocker = true;
                
            }
        }

        public static class SonsSdk
        {
            /// <summary>
            /// Immediately loads the game into a test environment world.
            /// </summary>
            /// <arg>--sdk.loadintomain</arg>
            public static bool LoadIntoMain { get; internal set; }
            
            /// <summary>
            /// Immediately loads the game into a savegame (specified by savegame id).
            /// </summary>
            /// <arg>--savegame</arg>
            /// <example>--savegame 1440719049</example>
            public static string LoadSaveGame { get; internal set; }
            
            internal static void Setup()
            {
                WithoutArg["sdk.loadintomain"] = () => LoadIntoMain = true;
                WithArg["savegame"] = (string arg) => LoadSaveGame = arg;
            }
        }

        public static class Console
        {
            public enum DisplayMode
            {
                NORMAL,
                MAGENTA,
                RAINBOW,
                RANDOMRAINBOW,
                LEMON
            };
            public static DisplayMode Mode { get; internal set; }
            public static bool CleanUnityLogs { get; internal set; } = true;
            public static bool ShouldSetTitle { get; internal set; } = true;
            public static bool AlwaysOnTop { get; internal set; }
            public static bool ShouldHide { get; internal set; }
            public static bool HideWarnings { get; internal set; }

            internal static void Setup()
            {
                WithoutArg["disableunityclc"] = () => CleanUnityLogs = false;
                WithoutArg["consoledst"] = () => ShouldSetTitle = false;
                WithoutArg["consoleontop"] = () => AlwaysOnTop = true;
                WithoutArg["hideconsole"] = () => ShouldHide = true;
                WithoutArg["hidewarnings"] = () => HideWarnings = true;

                WithArg["consolemode"] = (string arg) =>
                {
                    if (int.TryParse(arg, out int valueint))
                        Mode = (DisplayMode)LoaderUtils.Clamp(valueint, (int)DisplayMode.NORMAL, (int)DisplayMode.LEMON);
                };
            }
        }

        public static class Il2CppAssemblyGenerator
        {
            public static bool ForceRegeneration { get; internal set; }
            public static bool OfflineMode { get; internal set; }
            public static bool DisableDeobfMapIntegrityCheck { get; internal set; }
            public static string ForceVersion_Dumper { get; internal set; }
            public static string ForceRegex { get; internal set; }

            internal static void Setup()
            {
                WithoutArg["disabledmic"] = () => DisableDeobfMapIntegrityCheck = true;
                WithoutArg["agfoffline"] = () => OfflineMode = true;
                WithoutArg["agfregenerate"] = () => ForceRegeneration = true;
                WithArg["agfvdumper"] = (string arg) => ForceVersion_Dumper = arg;
                WithArg["agfregex"] = (string arg) => ForceRegex = arg;
            }
        }

        public static class Logger
        {
            public static int MaxLogs { get; internal set; } = 10;
            public static int MaxWarnings { get; internal set; } = 10;
            public static int MaxErrors { get; internal set; } = 10;

            internal static void Setup()
            {
                WithArg["maxlogs"] = (string arg) =>
                {
                    if (int.TryParse(arg, out int valueint))
                        MaxLogs = valueint;
                };
                WithArg["maxwarnings"] = (string arg) =>
                {
                    if (int.TryParse(arg, out int valueint))
                        MaxWarnings = valueint;
                };
                WithArg["maxerrors"] = (string arg) =>
                {
                    if (int.TryParse(arg, out int valueint))
                        MaxErrors = valueint;
                };
            }
        }
        #endregion
    }
}