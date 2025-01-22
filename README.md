
<p align="center">
  <a href="#"><img src="https://raw.githubusercontent.com/ToniMacaroni/SonsModLoader/master/Resources/redlogo.png" width="400"></a>
</p>

<p align="center">
	<a href="https://github.com/ToniMacaroni/RedLoader/releases/latest"><img src="https://img.shields.io/github/downloads/ToniMacaroni/SaberFactory/total?label=downloads&style=for-the-badge"></a>
	<a href="https://github.com/ToniMacaroni/RedLoader/releases/latest"><img src="https://img.shields.io/github/v/release/ToniMacaroni/SonsModLoader?label=latest&style=for-the-badge"></a>
</p>

---

<!-- <p>
	<h3 align="center"><a href="https://github.com/LavaGang/MelonLoader">Original Melonloader Repo</a></h3>
</p> -->

# RedLoader
**... is a mod loader and sdk for the game "Sons of the Forest". With In-Game configurations, UI frameworks and many APIs for the different systems in the game
it's main purpose is to make mod development for Sons Of The Forest more streamlined and making it a user friendly experience for players.**

---

:orange_book: **[Documentation and Wiki](https://tonimacaroni.github.io/RedLoader/)** :orange_book:

:star: **Star the repo at the top if you like this project** :star:

---

## FEATURES:
- Automatic asset bundle loading and mapping to class
- Automatic addressables catalog loading
- A powerful UI framework
- Status screen
- Toggle console at runtime
- Save and set console position
- Directly load into test scene
- Define mod using manifest.json
- Load sounds at runtime
- Quickly setup debug GUIs using attributes
- Bunch of helpers for game features

## INSTALLATION:
___
**Automatic**  
Download the **[RedManager](https://github.com/ToniMacaroni/RedManager)**.  
The RedManager is a single file (no installation needed) application that can detect your game installation
and install/update/remove RedLoader as well as other extras for you.
___

**Manual**
1. Download the latest release from [here](https://github.com/ToniMacaroni/RedLoader/releases/latest) (RedLoader.zip)
2. Unpack the zip directly into your game directory (the _Redloader folder should end up in the same directory as SonsOfTheForest.exe)
3. Make sure you have installed all requirements (listed below)
___

## Dedicated server installation
For docker images see https://github.com/ToniMacaroni/docker-redloader-sotf-server

**Windows:**  
Dedicated servers on windows should work out of the box. Just extract the RedLoader.zip into the server directory.

**Linux (Wine):**
1. Extract the RedLoader.zip into the server directory.
2. set the `WINEDLLOVERRIDES` environment to `"version=n,b"` (`export WINEDLLOVERRIDES="version=n,b"`)
3. Run the server as you would normally (`wine64 /sons/SonsOfTheForestDS.exe`).

## Configuration
### Command Line
*These can be added through the steam launch options or through a shortcut*

| Argument | Example | Description |
|:----------:|:---------:|:-------------:|
| `--sdk.loadintomain` |  | Immediately loads the game into a test environment world. |
| `--savegame` | `--savegame 1440719049` | Immediately loads the game into a savegame (specified by savegame id). |


### Config
*These can be changed in the `UserData/_Redloader.cfg` file*

| Entry | Type | Description |
|:----------:|:---------:|:-------------:|
| `Readable Exceptions` | `bool` | Makes the exceptions more readable. |
| `Disable Notifications` | `bool` | Disable the popup notifications. |
| `Auto Fix Reshade` | `bool` | Automatically rename dxgi.dll in the game folder to make Redloader able to load Reshade. |
| `Redirect Debug Logs` | `bool` | Redirect Debug Logs of the game to the console. |
| `Skip Intro` | `bool` | Skip the EndNight intro. |
| `Muted Sounds` | `List[string]` | List of sounds that should be muted. |
| `Toggle Console Key` | `KeyCode` | Key used to toggle the in-game console. |
| `Don't Auto Add Scenes` | `bool` | Indicates whether additional scenes should not be added automatically. |
| `Don't Load Saves` | `bool` | Indicates whether the game should skip the activation process and not load saves. |
| `Activate World Objects` | `bool` | Indicates whether world objects such as trees, bushes, and rocks should be activated or not. |
| `Player Debug Speed` | `float` | Multiplier for the speed of the player in the test world. |
| `Skip Building Animations` | `bool` | Indicates whether building animations should be skipped. |
| `Enable Bow Trajectory` | `bool` | Indicates whether the bow trajectory should be displayed when aiming. |
| `No Consume Animation` | `bool` | Whether to not play any animations when consuming items. |
| `No Auto Equip Stones` | `bool` | Don't automatically equip stones when picking them up. |
| `Instant Inventory Open` | `bool` | Instantly open the inventory without animations. |


### In-Game Debug Console
*These can be invoked through the in-game debug console (open with `F1` key)*

| Command | Example | Description |
|:----------:|:---------:|:-------------:|
| `togglegrass` |  | Toggles the visibility of grass |
| `xfreecam` |  | Freecam mode without "exiting" the player |
| `cancelblueprints` |  | Cancel all blueprints in a radius |
| `finishblueprints` |  | Finish all blueprints in a radius |
| `noforest` |  | Removes trees, bushes and (including billboards) for debugging purposes |
| `clearpickups` |  | Clears all pickups in a radius |
| `gotopickup` |  | Go to a pickup by name (picks the first one that contains the name). Useful for finding story items. |
| `aighostplayer` |  | Will make the ai ignore the player. |
| `saveconsolepos` |  | Save the console position to the config. |
| `virginiasentiment` |  | Add sentiment to virginia |
| `virginiavisit` |  | Invokes a virginia visit event |
| `dump` |  | Dump various data from the game. dump [items, characters, prefabs] |
| `playcutscene` |  | Play a cutscene by name |
| `toggleshadows` |  | Toggles the shadow rendering (Shadows, Contact Shadows, Micro Shadowing) |


### Compiling from source
1. Install .NET 6.0 SDK
2. Install Nuke globally with `dotnet tool install Nuke.GlobalTool --global`
3. Install Rust (tripple:`x86_64-pc-windows-msvc` nightly)
4. Clone the repository
5. Adjust the game path in `.nuke/parameters.json` and `Directory.Build.props`
6. Run `nuke pack --configuration Release --restore-packages` in the repo directory

## REQUIREMENTS:

- [.NET 6.0 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0#runtime-6.0.15)
- Microsoft Visual C++ 2015-2019 Re-distributable [[x64](https://aka.ms/vs/16/release/vc_redist.x64.exe)]

### LICENSING & CREDITS:

RedLoader is a fork of MelonLoader which is licensed under the Apache License, Version 2.0   
See LICENSING & CREDITS on the original [MelonLoader](https://github.com/LavaGang/MelonLoader) readme for the full list of credits and licenses.
