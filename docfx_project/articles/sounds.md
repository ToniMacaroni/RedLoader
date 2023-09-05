# Custom Sounds
## Loading directly via mp3/wav files
To load a sound from a file:
```csharp
SoundTools.RegisterSound("mysound", path);
SoundTools.PlaySound("mysound");
```
`mysound` is the id of the sound by which you can play it later. It needs to be unique.
You can also pass in the volume and pitch to the `PlaySound` method.

### 3D Sounds
To play a sound in 3D space you need to set the `is3d` parameter of the `RegisterSound` method to `true`.
Then you can use the `PlaySound` method with the `pos` parameter to play the sound at a specific position.
```csharp
SoundTools.RegisterSound("mysound", path, true);
SoundTools.PlaySound("mysound", new Vector3(0, 0, 0));
```

### 3D Sound attached to a gameobject
Manually setting the position might not really be what you want.
If you rather want to attach a sound to a gameobject and have it follow the object automatically, there is a better way.
```csharp
var go = new GameObject("Sound Player");
var player = go.AddComponent<SoundPlayer>();
player.Sound = SoundTools.GetSound("mysound");
player.Play();

player.ChannelDistance = 10; // You can set the distance like this.
```

## Loading FMOD Banks
RedLoader can also load events from FMOD Banks.  
**It's important to note that the banks have to be made in the [provided FMOD project](https://github.com/ToniMacaroni/SonsFModProject).**  
Once you have your master bank and optional other bank, you can load them in like this:
```csharp
SoundTools.LoadBank(@"Absolute\Path\To\My\Banks\master.bank");
```
Keep in mind the .strings.bank file is automatically loaded if it's in the same folder as the master bank.
To load banks from a buffer you can use the same method but with a byte array instead of a path.
After that the events are available to use like any other game event.

## Replacing game sounds
RedLoader can also replace sounds in the game with your own (or another sound from the game).
```csharp
SoundTools.SetupRedirect("event:/GameEvent", "event:/MyEvent");
```