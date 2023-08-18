using System;

namespace SFLoader
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class HarmonyDontPatchAllAttribute : Attribute { public HarmonyDontPatchAllAttribute() { } }
}