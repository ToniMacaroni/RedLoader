using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace SonsSdk;

public class MaterialTools
{
    public enum EDiffusionProfile
    {
        DefaultDiffusionProfile,
        SonsGrass,
        HumanSkinA,
        MutantSkin,
        HumanSkinVirginia,
        SonsFoliage,
        WaterDiffusionProfile,
        Jellyfish,
        SonsBillboards,
        HumanSkinB,
        SonsFoliageWinter,
        IceDiffusionProfile,
        CaveStalagmite,
        SmokeDiffusionProfile,
        SkinMutant
    }

    public static DiffusionProfileSettings GetDiffusionProfile(EDiffusionProfile profile)
    {
        return DiffusionProfileList.Value.TryGetValue(profile, out var result) ? result : null;
    }
    
    public static void SetDiffusionProfile(Material material, EDiffusionProfile profile)
    {
        var prof = GetDiffusionProfile(profile);
        if (!prof)
            return;
        
        material.SetFloat(DiffusionProfileHashId, HDShadowUtils.Asfloat(prof.profile.hash));
    }

    private static Dictionary<EDiffusionProfile, DiffusionProfileSettings> InitDiffusionProfiles()
    {
        var result = new Dictionary<EDiffusionProfile, DiffusionProfileSettings>();
        
        foreach (var profile in HDRenderPipeline.currentPipeline.m_SSSSetDiffusionProfiles)
        {
            if(!profile)
                continue;
            
            switch (profile.name)
            {
                case "defaultDiffusionProfile":
                    result.Add(EDiffusionProfile.DefaultDiffusionProfile, profile);
                    break;
                case "SonsGrass":
                    result.Add(EDiffusionProfile.SonsGrass, profile);
                    break;
                case "HumanSkinA":
                    result.Add(EDiffusionProfile.HumanSkinA, profile);
                    break;
                case "MutantSkin":
                    result.Add(EDiffusionProfile.MutantSkin, profile);
                    break;
                case "HumanSkinVirginia":
                    result.Add(EDiffusionProfile.HumanSkinVirginia, profile);
                    break;
                case "SonsFoliage":
                    result.Add(EDiffusionProfile.SonsFoliage, profile);
                    break;
                case "Water Diffusion Profile":
                    result.Add(EDiffusionProfile.WaterDiffusionProfile, profile);
                    break;
                case "Jellyfish":
                    result.Add(EDiffusionProfile.Jellyfish, profile);
                    break;
                case "SonsBillboards":
                    result.Add(EDiffusionProfile.SonsBillboards, profile);
                    break;
                case "HumanSkinB":
                    result.Add(EDiffusionProfile.HumanSkinB, profile);
                    break;
                case "SonsFoliageWinter":
                    result.Add(EDiffusionProfile.SonsFoliageWinter, profile);
                    break;
                case "Ice Diffusion Profile":
                    result.Add(EDiffusionProfile.IceDiffusionProfile, profile);
                    break;
                case "CaveStalagmite":
                    result.Add(EDiffusionProfile.CaveStalagmite, profile);
                    break;
                case "SmokeDiffusionProfile":
                    result.Add(EDiffusionProfile.SmokeDiffusionProfile, profile);
                    break;
                case "SkinMutant":
                    result.Add(EDiffusionProfile.SkinMutant, profile);
                    break;
            }
        }
        
        return result;
    }

    private static Lazy<Dictionary<EDiffusionProfile, DiffusionProfileSettings>> DiffusionProfileList =
        new(InitDiffusionProfiles);
    
    private static readonly int DiffusionProfileAssetId = Shader.PropertyToID("_DiffusionProfileAsset");
    private static readonly int DiffusionProfileHashId = Shader.PropertyToID("_DiffusionProfileHash");
}