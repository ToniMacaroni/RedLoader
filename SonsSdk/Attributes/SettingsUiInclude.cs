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