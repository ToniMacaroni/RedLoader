using Endnight.Types;
using FMOD;
using RedLoader;
using Sons.Gui;
using Sons.Input;
using Sons.Save;
using SonsSdk.Exceptions;
using TheForest.Utils;
using UnityEngine;

namespace SonsSdk;

public static partial class SonsTools
{
    /// <summary>
    /// Show a message in the bottom left corner of the screen for a certain duration.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="duration"></param>
    public static void ShowMessage(string message, float duration = 3f)
    {
        if (!HudGui._instance)
            return;
        
        HudGui._instance.DisplayGeneralMessage(message, duration);
    }

    /// <summary>
    /// Toggle menu mode. Stops all keyboard inputs and shows the cursor.
    /// </summary>
    /// <param name="enable"></param>
    public static void MenuMode(bool enable)
    {
        InputSystem.SetState(InputState.Console, enable);
    }
    
    /// <summary>
    /// Get a vector3 position in from of the player
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    public static Vector3 GetPositionInFrontOfPlayer(float distance)
    {
        if (!LocalPlayer.IsInWorld)
            throw new NotInWorldException();
        
        var t = LocalPlayer.Transform;
        return t.position + t.forward * distance;
    }

    /// <summary>
    /// Get a vector3 position in from of the player
    /// </summary>
    /// <param name="distance"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static Vector3 GetPositionInFrontOfPlayer(float distance, float height)
    {
        if(!LocalPlayer.IsInWorld)
            throw new NotInWorldException();
        
        var t = LocalPlayer.Transform;
        return t.position + t.forward * distance + t.up * height;
    }

    /// <summary>
    /// Get all savegame ids for a specific savegame type
    /// </summary>
    /// <param name="saveIds"></param>
    /// <param name="saveGameType"></param>
    /// <returns></returns>
    public static bool TryGetSaveGameIds(out List<uint> saveIds, SaveGameType saveGameType = SaveGameType.SinglePlayer)
    {
        saveIds = new List<uint>();
        if (!SingletonBehaviour<SaveGameManager>.TryGetInstance(out var _))
        {
            RLog.Error("SaveGameManager not initialized, aborting get.");
            return false;
        }
        var saveGameFolder = SaveGameManager.GetSaveGameFolder(saveGameType) ?? "";
        var length = saveGameFolder.Length;
        if (!Directory.Exists(saveGameFolder))
        {
            RLog.Error("[SaveGameManager] No save folder found " + saveGameFolder);
            return false;
        }
        string[] directories = Directory.GetDirectories(saveGameFolder);
        foreach (string dir in directories)
        {
            if (File.Exists(dir + "/SaveData.json") && uint.TryParse(dir.Substring(length + 1), out var result))
            {
                saveIds.Add(result);
            }
        }
        return saveIds.Count > 0;
    }
    
    public static uint GetLatestSaveGameId(SaveGameType saveGameType = SaveGameType.SinglePlayer)
    {
        if (!TryGetSaveGameIds(out var saveIds, saveGameType))
            return 0;

        var latest = DateTime.MinValue;
        uint latestId = 0;
        
        foreach (var saveId in saveIds)
        {
            var file = new FileInfo(SaveGameManager.GetSaveGameFolder(saveGameType) + "/" + saveId + "/SaveData.json");
            if (file.LastWriteTime > latest)
            {
                latest = file.LastWriteTime;
                latestId = saveId;
            }
        }
        
        return latestId;
    }

    public static void ShowMessageBox(string title, string message)
    {
        GenericModalDialog.ShowDialog(title, message);
    }
}