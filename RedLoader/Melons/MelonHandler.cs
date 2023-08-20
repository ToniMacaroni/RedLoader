using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using RedLoader.Fixes;
using RedLoader.Utils;

namespace RedLoader
{
    public static class MelonHandler
    {
        internal static void Setup()
        {
            if (!Directory.Exists(MelonEnvironment.CoreModDirectory))
                Directory.CreateDirectory(MelonEnvironment.CoreModDirectory);
            
            if (!Directory.Exists(MelonEnvironment.PluginsDirectory))
                Directory.CreateDirectory(MelonEnvironment.PluginsDirectory);
            
            if (!Directory.Exists(MelonEnvironment.ModsDirectory))
                Directory.CreateDirectory(MelonEnvironment.ModsDirectory);
        }

        private static bool firstSpacer = false;
        public static void LoadMelonsFromDirectory<T>(string path) where T : MelonTypeBase<T>
        {
            path = Path.GetFullPath(path);

            var loadingMsg = $"Loading {MelonTypeBase<T>.TypeName}s from '{path}'...";
            RLog.WriteSpacer();
            RLog.Msg(loadingMsg);

            bool hasWroteLine = false;

            var files = Directory.GetFiles(path, "*.dll");
            var melonAssemblies = new List<MelonAssembly>();
            foreach (var f in files)
            {
                var newFile = OldMelonFixer.Fix(f);
                
                if (!hasWroteLine)
                {
                    hasWroteLine = true;
                    RLog.WriteLine(Color.Magenta);
                }

                var asm = MelonAssembly.LoadMelonAssembly(newFile, false);
                if (asm == null)
                    continue;

                melonAssemblies.Add(asm);
            }

            var melons = new List<T>();
            foreach (var asm in melonAssemblies)
            {
                asm.LoadMelons();
                foreach (var m in asm.LoadedMelons)
                {
                    if (m is T t)
                    {
                        melons.Add(t);
                    }
                    else
                    {
                        RLog.Warning($"Failed to load Melon '{m.Info.Name}' from '{path}': The given Melon is a {m.MelonTypeName} and cannot be loaded as a {MelonTypeBase<T>.TypeName}. Make sure it's in the right folder.");
                        continue;
                    }
                }
            }

            if (hasWroteLine)
                RLog.WriteSpacer();

            MelonBase.RegisterSorted(melons);

            if (hasWroteLine)
                RLog.WriteLine(Color.Magenta);

            var count = MelonTypeBase<T>._registeredMelons.Count;
            RLog.Msg($"{count} {MelonTypeBase<T>.TypeName.MakePlural(count)} loaded.");
            if (firstSpacer || (typeof(T) ==  typeof(MelonMod)))
                RLog.WriteSpacer();
            firstSpacer = true;
        }
        
        public static void LoadModsFromDirectory(string path, string name)
        {
            path = Path.GetFullPath(path);

            var loadingMsg = $"Loading {name}s from '{path}'...";
            RLog.WriteSpacer();
            RLog.Msg(loadingMsg);

            bool hasWroteLine = false;

            var files = Directory.GetFiles(path, "*.dll");
            var melonAssemblies = new List<MelonAssembly>();
            foreach (var f in files)
            {
                var newFile = OldMelonFixer.Fix(f);
                
                if (!hasWroteLine)
                {
                    hasWroteLine = true;
                    RLog.WriteLine(Color.Magenta);
                }

                var asm = MelonAssembly.LoadMelonAssembly(newFile, false);
                if (asm == null)
                    continue;

                melonAssemblies.Add(asm);
            }

            var melons = new List<MelonBase>();
            foreach (var asm in melonAssemblies)
            {
                asm.LoadMelons();
                foreach (var m in asm.LoadedMelons)
                {
                    if (m is not MelonPlugin)
                    {
                        melons.Add(m);
                    }
                }
            }

            if (hasWroteLine)
                RLog.WriteSpacer();

            MelonBase.RegisterSorted(melons);

            if (hasWroteLine)
                RLog.WriteLine(Color.Magenta);

            var count = MelonBase._registeredMelons.Count;
            RLog.Msg($"{count} {name.MakePlural(count)} loaded.");
            RLog.WriteSpacer();
            firstSpacer = true;
        }
    }
}
