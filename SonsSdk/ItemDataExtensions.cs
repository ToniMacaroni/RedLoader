using HarmonyLib;
using Sons.Items.Core;
using UnityEngine;
using Types = Sons.Items.Core.Types;

namespace SonsSdk;

public static class ItemDataExtensions
{
    public static void SetupHeld(
        this ItemData item,
        EquipmentSlot slot,
        AnimatorVariables[] animVars,
        ItemUiData.LeftClickCommands leftClick = ItemUiData.LeftClickCommands.equip,
        ItemUiData.RightClickCommands rightClick = ItemUiData.RightClickCommands.None,
        ItemData.GuiType guiType = ItemData.GuiType.Weapon)
    {
        item._equipmentSlot = slot;
        item._equippedAnimVars = animVars;
        item._type |= Types.Equippable;
        item._guiType = guiType;
        item._uiData._leftClick = leftClick;
        item._uiData._rightClick = rightClick;
    }

    public static void SetupClickActions(this ItemData item,
                                         ItemUiData.LeftClickCommands leftClick,
                                         ItemUiData.RightClickCommands rightClick)
    {
        item._uiData._leftClick = leftClick;
        item._uiData._rightClick = rightClick;
    }

    public static void SetIcon(this ItemData item, Texture2D tex)
    {
        item._uiData._icon = item._uiData._outlineIcon = tex;
    }
}
