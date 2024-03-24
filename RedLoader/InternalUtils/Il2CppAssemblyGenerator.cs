using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using RedLoader.Modules;
using RedLoader.Utils;

namespace RedLoader.InternalUtils
{
    internal static class Il2CppAssemblyGenerator
    {
        public static readonly MelonModule.Info moduleInfo = new MelonModule.Info(
            $"{LoaderEnvironment.LoaderFolderName}{Path.DirectorySeparatorChar}Dependencies{Path.DirectorySeparatorChar}Il2CppAssemblyGenerator{Path.DirectorySeparatorChar}Il2CppAssemblyGenerator.dll"
            , () => !LoaderUtils.IsGameIl2Cpp());

        internal static bool Run()
        {
            var module = MelonModule.Load(moduleInfo);
            if (module == null)
                return true;
            
            RLog.MsgDirect("Loading Il2CppAssemblyGenerator...");
            if (RConsole.ShouldHandleConsole)
            {
                IntPtr windowHandle = Process.GetCurrentProcess().MainWindowHandle;
                BootstrapInterop.DisableCloseButton(windowHandle);
            }
            
            var ret = module.SendMessage("Run");
            
            if (RConsole.ShouldHandleConsole)
            {
                IntPtr windowHandle = Process.GetCurrentProcess().MainWindowHandle;
                BootstrapInterop.EnableCloseButton(windowHandle);
            }
            return ret is 0;
        }
    }
}