using Endnight.Types;
using Endnight.Utilities;
using FMOD;
using Il2CppSystem.Linq;
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
    /// Calculate the distance between the player and a position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public static float GetPlayerDistance(Vector3 position)
    {
        return Vector3.Distance(ActiveWorldLocation.Position, position);
    }
    
    /// <summary>
    /// Check if the player is under a certain distance to a position
    /// </summary>
    /// <param name="position"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public static bool IsPlayerInDistance(Vector3 position, float distance)
    {
        return GetPlayerDistance(position) <= distance;
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
    
    public static IEnumerable<T> FilterOnDistance<T>(this IEnumerable<T> source, float distance) where T : Component
    {
        return source.Where(x => Vector3.Distance(x.transform.position, ActiveWorldLocation.Position) <= distance);
    }
}