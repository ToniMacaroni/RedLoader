using System.Drawing;
using System.Reflection;
using RedLoader;
using RedLoader.Utils;
using SonsSdk;
using SonsSdk.Attributes;

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

        var path = MelonEnvironment.GetMetadataPath(melonAssembly.Assembly);
        var manifest = ManifestReader.TryReadManifest(path);
        if (manifest != null)
        {
            if (InitMod(melonAssembly, manifest, out var mod))
            {
                RLog.Msg(System.ConsoleColor.Magenta, $"Loaded mod {mod.Info.Name}");
                
                if(melons.FindIndex(x=>x.ID == mod.ID) != -1)
                {
                    RLog.Error($"Mod Id collision detected! \"{mod.ID}\" will not be loaded.");
                }
                else
                {
                    melons.Add(mod);

                    mod.AssetBundleAttrs = AssetBundleAttributeLoader.GetAllTypes(mod);
                }
            }
        }
        else
        {
            RLog.Error($"{assembly.FullName} does not have a manifest.json file.");
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
            RLog.Error($"Failed to find a valid implementation of SonsMod in {assembly.FullName}");
            return false;
        }
        
        try
        {
            outMod = (SonsMod)Activator.CreateInstance(implementation, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, null, null);
            
            if(outMod == null)
            {
                RLog.Error($"Failed to create an instance of {implementation.FullName}");
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
            outMod.AuthorConsoleColor = RLog.DefaultTextColor;
            outMod.SupportedGameVersion = data.GameVersion;
            outMod.OptionalDependencies = data.Dependencies;
            outMod.ID = data.Id;
            melonAssembly.HarmonyDontPatchAll = data.DontApplyPatches;
            
            return true;
        }
        catch (Exception e)
        {
            RLog.Error(e);
            return false;
        }
    }
}