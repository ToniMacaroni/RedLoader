# Creating Mods
This article will cover how to setup a mod project using the [RedManager](https://github.com/ToniMacaroni/RedManager) as it's the easiest way. If you want to use the template directory you can see it [here](https://github.com/ToniMacaroni/RedLoader.Templates).

## Creating the project (RedManager)

1. Start up the RedManager
2. If you path isn't correctly set, adjust it
3. Head over to the `Modders` tab
4. Click on `Install template` if you haven't installed the template yet
5. Put in your mod name and click `Create Project`
6. Select the target folder for your mod (a subfolder with your mod name will be created in the target folder)

## Creating the project (CLI)
If you want to use the command line to create your project look at the readme [here](https://github.com/ToniMacaroni/RedLoader.Templates).

## IMPORTANT
The RedManager can't update the template once a new one comes out. If you want to update the template open up the console and execute `dotnet new install RedLoader.Templates`.

## Project description
The project includes several features out of the box:
- The mod will automatically be copied to the game on build
- If you use Rider you can press `Ctrl+F5` to automatically copy the mod and run the game

If you set the mode to release it will:
- Automatically create a folder with the correct mod structure
- Automatically create a zip with the created folder

The template also sets up a basic mod for you. You should be able to just build the mod and see the mod being loaded.

### Adjusting the mod info
The first thing you should do is adjust your mod info (like author, mod name etc.) in the `manifest.json` file. The file is linked to a valid json schema, so if you are ever unsure what you can write in there just press `Ctrl+Space` to open autocomplete.

## Tips
If you want to automatically apply harmony patches for your assembly add `HarmonyPatchAll = true;` to your constructor (or uncomment it).

If you want to subscribe to unity's `OnUpdate`, `OnFixedUpdate`, `OnGUI` and `OnLateUpdate` message queue you can register it in the constructor by for example using `OnUpdateCallback = MyMethod`.