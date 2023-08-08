using System.Drawing;
using System.Reflection;
using MelonLoader;
using SonsSdk;

[assembly: MelonInfo(typeof(SonsLoaderPlugin.SonsLoaderPlugin), "SonsLoaderPlugin", "1.0.0", "Sons")]
[assembly: MelonColor(220, 150, 0, 255)]

namespace SonsLoaderPlugin;

public class SonsLoaderPlugin : MelonPlugin
{
    public override void OnPreModsLoaded()
    {
        MelonAssembly.CustomMelonResolvers += Resolver;
    }

    public override void OnLateInitializeMelon()
    {
        SdkEvents.Init();
    }

    private ResolvedMelons Resolver(MelonAssembly melonAssembly)
    {
        var assembly = melonAssembly.Assembly;
        
        var melons = new List<MelonBase>();
        var rottenMelons = new List<RottenMelon>();
        
        var info = MelonUtils.PullAttributeFromAssembly<SonsModInfoAttribute>(assembly);
        if (info != null && info.SystemType != null && info.SystemType.IsSubclassOf(typeof(SonsMod)))
        {
            SonsMod mod;
            try
            {
                mod = (SonsMod)Activator.CreateInstance(info.SystemType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, null, null);
            }
            catch (Exception ex)
            {
                mod = null;
                rottenMelons.Add(new RottenMelon(info.SystemType, "Failed to create an instance of the Melon.", ex));
            }
            
            if (mod != null)
            {
                mod.Info = info;
                melons.Add(mod);
            }
        }
        
        return new ResolvedMelons(melons.ToArray(), rottenMelons.ToArray());
    }
}