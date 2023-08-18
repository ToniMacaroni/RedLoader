using System;

namespace RedLoader
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class HarmonyDontPatchAllAttribute : Attribute { public HarmonyDontPatchAllAttribute() { } }
}