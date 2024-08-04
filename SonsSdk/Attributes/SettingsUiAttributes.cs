using TMPro;
using UnityEngine;

namespace SonsSdk.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class SettingsUiMode : Attribute
{
    public enum ESettingsUiMode
    {
        OptIn,
        OptOut
    }
    
    public ESettingsUiMode Mode { get; }
    
    public SettingsUiMode(ESettingsUiMode mode)
    {
        Mode = mode;
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SettingsUiInclude : Attribute
{ }

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SettingsUiIgnore : Attribute
{ }

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SettingsUiHeader : Attribute
{
    public string Text { get; }
    public Color? Color { get; }
    public TextAlignmentOptions Alignment { get; }
    public bool LightFont { get; }
    
    public SettingsUiHeader(string text, TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft, bool lightFont = true)
    {
        Text = text;
        Alignment = alignment;
        LightFont = lightFont;
    }

    public SettingsUiHeader(string text, string color, TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft, bool lightFont = true)
    {
        Text = text;
        Color = SUI.SUI.ColorFromString(color);
        Alignment = alignment;
        LightFont = lightFont;
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SettingsUiSpacing : Attribute
{
    public float Spacing { get; }
    
    public SettingsUiSpacing(float spacing)
    {
        Spacing = spacing;
    }
}