# Configuration

To allow users to configure parameters of your mod you can add config entries to your mod.
To do so take a look at the following example:
```csharp
public static class Config
{
    public static ConfigCategory Category { get; private set; }
    
    public static ConfigEntry<float> SomeValue { get; private set; }

    public static void Init()
    {
        Category = ConfigSystem.CreateFileCategory("Zippy", "Zippy", "Zippy.cfg");

        SomeValue = Category.CreateEntry(
            "display_depth",
            0.0692f,
            "Display Depth",
            "Position of the display on the barrel axis.");
        DisplayDepth.SetRange(-0.03f,0.2f);
    }
}
```

First you need to create a category for your config. You can do so with `ConfigSystem.CreateFileCategory(id, displayName, fileName);`.  
Once you have a category you can add entries to it. To do so use `Category.CreateEntry(id, defaultValue, displayName, description);`.  
Optionally you can set a range for numeric entries and options for enum entries. You would then call `Init()` in the `OnSdkInitialized()` method of your mod.

### Input config entries

Redloader comes with a custom configuration system for the new input system. The configuration class will look almost the same.
```csharp
public static class Config
{
    public static ConfigCategory Category { get; private set; }
    
    public static KeybindConfigEntry SomeKey { get; private set; }

    public static void Init()
    {
        Category = ConfigSystem.CreateFileCategory("Zippy", "Zippy", "Zippy.cfg");
        
        SomeKey = Category.CreateKeybindEntry("key", "g", "Key", "Some key");
    }
}
```

You can then register action for your key anywhere with `Config.SomeKey.Notify(MyAction, MyOptionalReleaseAction);`