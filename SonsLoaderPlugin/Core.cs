using System.Drawing;
using System.Reflection;
using MelonLoader;
using SonsSdk;

[assembly: MelonInfo(typeof(SonsLoaderPlugin.Core), "SonsLoaderPlugin", "1.0.0", "Toni Macaroni")]
[assembly: MelonColor(0, 255, 20, 255)]

namespace SonsLoaderPlugin;

public class Core : MelonPlugin
{
    public override void OnPreModsLoaded()
    {
        MelonAssembly.CustomMelonResolvers += Resolver;
    }

    public override void OnLateInitializeMod()
    {
        SdkEvents.Init();
    }

    private ResolvedMelons Resolver(MelonAssembly melonAssembly)
    {
        var assembly = melonAssembly.Assembly;
        
        var melons = new List<MelonBase>();
        var rottenMelons = new List<RottenMelon>();
        
        // var info = MelonUtils.PullAttributeFromAssembly<SonsModInfoAttribute>(assembly);
        // if (info != null && info.SystemType != null && info.SystemType.IsSubclassOf(typeof(SonsMod)))
        // {
        //     SonsMod mod;
        //     try
        //     {
        //         mod = (SonsMod)Activator.CreateInstance(info.SystemType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, null, null);
        //     }
        //     catch (Exception ex)
        //     {
        //         mod = null;
        //         rottenMelons.Add(new RottenMelon(info.SystemType, "Failed to create an instance of the Melon.", ex));
        //     }
        //     
        //     if (mod != null)
        //     {
        //         mod.Info = info;
        //         melons.Add(mod);
        //     }
        // }
        
        var manifest = ManifestReader.TryReadManifest(assembly);
        if (manifest != null)
        {
            if (InitMod(melonAssembly, manifest, out var mod))
            {
                MelonLogger.Msg(System.ConsoleColor.Magenta, $"Loaded mod {mod.Info.Name}");
                melons.Add(mod);
            }
        }
        else
        {
            MelonLogger.Msg(System.ConsoleColor.Red, $"{assembly.FullName} does not have a manifest.json file.");
        }
        
        return new ResolvedMelons(melons.ToArray(), rottenMelons.ToArray());
    }

    private bool InitMod(MelonAssembly melonAssembly, ManifestData data, out SonsMod outMod)
    {
        outMod = null;
        
        var assembly = melonAssembly.Assembly;
        var implementation = assembly.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(SonsMod)));
        if (implementation == null)
        {
            MelonLogger.Error($"Failed to find a valid implementation of SonsMod in {assembly.FullName}");
            return false;
        }
        
        try
        {
            outMod = (SonsMod)Activator.CreateInstance(implementation, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, null, null);
            
            if(outMod == null)
            {
                MelonLogger.Error($"Failed to create an instance of {implementation.FullName}");
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
                
            outMod.MelonAssembly = melonAssembly;
            outMod.Priority = data.Priority;
            outMod.ConsoleColor = MelonUtils.ColorFromString(data.LogColor);
            outMod.AuthorConsoleColor = MelonLogger.DefaultTextColor;
            outMod.SupportedGameVersion = data.GameVersion;
            outMod.OptionalDependencies = data.Dependencies;
            outMod.ID = data.Id;
            melonAssembly.HarmonyDontPatchAll = data.DontApplyPatches;
            
            return true;
        }
        catch (Exception e)
        {
            MelonLogger.Error(e);
            return false;
        }
    }
}