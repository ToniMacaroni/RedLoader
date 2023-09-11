using FMOD;
using RedLoader;
using Sons.Gui;
using Sons.Input;
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

    public static void ShowMessageBox(string title, string message)
    {
        GenericModalDialog.ShowDialog(title, message);
    }
}