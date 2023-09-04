
<p align="center">
  <a href="#"><img src="https://raw.githubusercontent.com/ToniMacaroni/SonsModLoader/master/Resources/redlogo.png" width="400"></a>
</p>

<p align="center">
	<a href="https://github.com/ToniMacaroni/RedLoader/releases/latest"><img src="https://img.shields.io/github/v/release/ToniMacaroni/SonsModLoader?label=latest&style=for-the-badge"></a>
</p>

---

<!-- <p>
	<h3 align="center"><a href="https://github.com/LavaGang/MelonLoader">Original Melonloader Repo</a></h3>
</p> -->

# RedLoader
**... is a mod loader and sdk based on [MelonLoader](https://github.com/LavaGang/MelonLoader).
It's main purpose is to make mod development for Sons Of The Forest more streamlined by adding some much needed improvements and features.
As well as making it a bit more user friendly.**

---

:orange_book: **[Documentation and Wiki](https://tonimacaroni.github.io/RedLoader/)** :orange_book:

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
2. Unpack the zip directly into your game directory (the _RedLoader folder should end up in the same directory as SonsOfTheForest.exe)
3. Make sure you have installed all requirements (listed below)
___

## Configuration
### Command Line
*These can be added through the steam launch options or through a shortcut*

{argtable}

### Config
*These can be changed in the `UserData/_RedLoader.cfg` file*

{configtable}

### In-Game Debug Console
*These can be invoked through the in-game debug console (open with `F1` key)*

{commandtable}

## REQUIREMENTS:

- [.NET Framework 3.5 Runtime](https://www.microsoft.com/en-us/download/details.aspx?id=21)
- [.NET Framework 4.7.2 Runtime](https://dotnet.microsoft.com/download/dotnet-framework/net472)
- [.NET Framework 4.8 Runtime](https://dotnet.microsoft.com/download/dotnet-framework/net48)
- [.NET 6.0 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0#runtime-6.0.15)
- Microsoft Visual C++ 2015-2019 Re-distributable [[x64](https://aka.ms/vs/16/release/vc_redist.x64.exe)]

### LICENSING & CREDITS:

RedLoader is a fork of MelonLoader which is licensed under the Apache License, Version 2.0   
See LICENSING & CREDITS on the original [MelonLoader](https://github.com/LavaGang/MelonLoader) readme for the full list of credits and licenses.
