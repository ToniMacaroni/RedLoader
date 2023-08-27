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
            if (!Directory.Exists(LoaderEnvironment.CoreModDirectory))
                Directory.CreateDirectory(LoaderEnvironment.CoreModDirectory);
            
            if (!Directory.Exists(LoaderEnvironment.PluginsDirectory))
                Directory.CreateDirectory(LoaderEnvironment.PluginsDirectory);
            
            if (!Directory.Exists(LoaderEnvironment.ModsDirectory))
                Directory.CreateDirectory(LoaderEnvironment.ModsDirectory);
        }

        private static bool firstSpacer = false;
        public static void LoadMelonsFromDirectory<T>(string path) where T : ModTypeBase<T>
        {
            path = Path.GetFullPath(path);

            var loadingMsg = $"Loading {ModTypeBase<T>.TypeName}s from '{path}'...";
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
                        RLog.Warning($"Failed to load Melon '{m.Info.Name}' from '{path}': The given Melon is a {m.MelonTypeName} and cannot be loaded as a {ModTypeBase<T>.TypeName}. Make sure it's in the right folder.");
                        continue;
                    }
                }
            }

            if (hasWroteLine)
                RLog.WriteSpacer();

            ModBase.RegisterSorted(melons);

            if (hasWroteLine)
                RLog.WriteLine(Color.Magenta);

            var count = ModTypeBase<T>._registeredMelons.Count;
            RLog.Msg($"{count} {ModTypeBase<T>.TypeName.MakePlural(count)} loaded.");
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

            var melons = new List<ModBase>();
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

            ModBase.RegisterSorted(melons);

            if (hasWroteLine)
                RLog.WriteLine(Color.Magenta);

            var count = ModBase._registeredMelons.Count;
            RLog.Msg($"{count} {name.MakePlural(count)} loaded.");
            RLog.WriteSpacer();
            firstSpacer = true;
        }
    }
}
