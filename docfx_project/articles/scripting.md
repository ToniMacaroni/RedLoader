# Hotreloadable mods (Scripts)
Redloader allows you to quickly test code by using special mods (inside the `Scripts` folder) that can be reloaded at runtime.
When building the mod, redloader sees that the assembly has changed and automatically reloads the mod.

1) Enable scripts by setting `enable_script_loader` to true in the `Sons Of The Forest\UserData\_Redloader.cfg` config file.
2) Create a mod project like you would usually do, remove the existing mod class and add a class that inherits from `RedScript` like this.
```csharp
public class MyScriptTest : RedScript
{
    public override void OnLoad()
    {
        RLog.Msg(Color.GreenYellow, $"Loaded {nameof(ScriptTest)}");
    }

    public override void OnUnload()
    {
        RLog.Msg(Color.Orange, $"Unloaded {nameof(ScriptTest)}");
    }
}
```
`OnLoad` and `OnUnload` will be called everytime the mod reloads. Use `OnUnload` to clean up any stuff like unpatching harmony patches or removing gameobjects.
3) Since the mod needs to be build to the `Scripts` folder and doesn't need a `manifest.json` we will need to adjust the `Directory.Build.targets` file a bit. Replace the content in the file with the following:
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project>

    <UsingTask TaskName="CustomAfterBuild" AssemblyFile="$(TargetPath)" />

    <Target Name="CopyToGame" AfterTargets="Build" Condition="'$(DisableCopyToGame)' != 'True'">
        <PropertyGroup>
            <OutputAssemblyName>$(OutputPath)$(AssemblyName)</OutputAssemblyName>
            <CanCopy>True</CanCopy>
            <CopyError Condition="!Exists('$(GameDir)')">Unable to copy assembly to game folder. Game directory doesn't exist</CopyError>
            <CanCopy Condition="'$(CopyError)' != ''">False</CanCopy>
        </PropertyGroup>

        <Warning Text="$(CopyError)" Condition="'$(CopyError)' != ''"/>
        <Message Text="Copying '$(AssemblyName)' to '$(GameDir)'." Importance="high" Condition="$(CanCopy)"/>
        <Copy SourceFiles="$(OutputAssemblyName).dll" DestinationFiles="$(GameDir)\Scripts\$(AssemblyName).dll" Condition="$(CanCopy)"/>
        <Copy SourceFiles="$(OutputAssemblyName).pdb" DestinationFiles="$(GameDir)\Scripts\$(AssemblyName).pdb" Condition="'$(CanCopy)' AND Exists('$(OutputAssemblyName).pdb')"/>
    </Target>
</Project>
```
When building the script mod, it now will be built to the `Scripts` folder and automatically reloaded (check the console after building).

## Additional Info
Keep in mind that the scripts will be loaded pretty early upon starting the game (before `OnSdkInitialized` is being called).
So make sure to include checks, so the code doesn't run too early.

If you want to run the code only when the player is in the world, you should add a check like this:
```csharp
public override void OnLoad()
{
    RLog.Msg(Color.GreenYellow, $"Loaded {nameof(ScriptTest)}");
    
    if (!LocalPlayer._instance)
        return;
    
    // Add your code here...
}
```
