# Misc Loader Features
## Quick Loading
### LoadIntoMain
Load straight into a world for testing purposes by adding `--sdk.loadintomain` to your launch options.
You can configure what gets loaded in the `UserData/_RedLoader.cfg` file.

### LoadSave
You can directly load into a save game with the `--savegame <id>` launch option.  
You can find the ids of your save games in the `C:\Users\<user>\AppData\LocalLow\Endnight\SonsOfTheForest\Saves\<userid>\SinglePlayer` directory.

## Boot.txt
You can create a `boot.txt` in your game directory with a command on each line to be executed on game start.

## Debug commands
### Dump info
There are several things the loader allows you to dump from the game. To do so you can use the `dump` console command.
The dump will be written to the game folder in a file called `<type>.txt` (do for `dump items` it will be `items.txt`).
The following thing can be dumped currently:
- `dump items`: Lists all items
- `dump characters`: Lists all characters and their variations
- `dump prefabs` Lists all prefabs