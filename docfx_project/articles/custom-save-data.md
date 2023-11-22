# Adding custom data to a save file
Red Loader has utilities if you want your mod to persist data in save games. 
This is useful if you want to save settings or other data that you want to persist between game sessions for a particular save.

## Serializer class
Your mod needs some kind of class to serialize and deserialize your data.
A common way is to have some kind of "manager" class that holds all the data you want to save.

```csharp
public class MyModManager : ICustomSaveable<MyModManager.MyModSaveData>
{
    public string Name => "MyModManager";
    
    // Used to determine if the data should also be saved in multiplayer saves
    public bool IncludeInPlayerSave => true;

    public MyModSaveData Save()
    {
        // Serialize your data from game state here
        return new MyModSaveData();
    }

    public void Load(MyModSaveData obj)
    {
        // Apply game state from your data here
    }

    public class MyModSaveData
    {
        public Color SomeColor;
        public Vector3 SomeVector;
    }
}
```

The important parts here are the `ICustomSaveable` interface that `MyModManager` implements, and the `MyModSaveData` class that is used to serialize and deserialize the data.
The `Save` and `Load` methods from the `ICustomSaveable` interface are used to serialize and deserialize the data.
Red Loader will call them when a save game gets loaded or the user requests a save.

## Registering your serializer
Registering the serializer is as easy as calling the following in `OnSdkInitialized`:
```csharp
var manager = new MyModManager();
SonsSaveTools.Register(manager);
```