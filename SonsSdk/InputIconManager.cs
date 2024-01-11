using Sons.Gui.Input;
using SUI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SonsSdk;

public class InputIconManager
{
    private static IconAssetDatabase _iconDatabase;

    public static IconAssetDatabase IconDatabase
    {
        get
        {
            if(!_iconDatabase)
                Initialize();
            
            return _iconDatabase;
        }
    }

    private static void Initialize()
    {
        _iconDatabase = Resources.FindObjectsOfTypeAll<IconAssetDatabase>().First();
    }

    public static GameObject Get(InputAction inputAction, out string displayName, out bool isDefaultPrefab)
    {
        if (inputAction.controls.Count == 0)
        {
            displayName = null;
            isDefaultPrefab = false;
            return null;
        }
        
        var path = inputAction.controls[0].path;
        return Get(path, out displayName, out isDefaultPrefab);
    }
    
    public static GameObject Get(string path, out string displayName, out bool isDefaultPrefab)
    {
        if (string.IsNullOrEmpty(path))
        {
            displayName = null;
            isDefaultPrefab = false;
            return null;
        }
        
        var icon = IconDatabase.GetIcon(path, out isDefaultPrefab);
        if (icon != null)
        {
            var inputControl = InputSystem.FindControl(path);
            displayName = inputControl != null ? inputControl.displayName : "NA";
            var newIcon = UnityEngine.Object.Instantiate(icon);
            
            if (isDefaultPrefab)
            {
                var tmpText = newIcon.GetComponentInChildren<TMP_Text>(true);
                if (tmpText != null)
                {
                    tmpText.text = displayName;
                }
            }
            
            return newIcon;
        }
        
        displayName = null;
        isDefaultPrefab = false;
        return null;
    }

    public static GameObject AddInputIcon(string path, SUiElement element)
    {
        return AddInputIcon(path, element.Root.transform);
    }
    
    public static GameObject AddInputIcon(string path, Transform transform)
    {
        var inputIcon = Get(path, out _, out _);
        if (!inputIcon)
        {
            return null;
        }
        
        inputIcon.SetActive(true);
        inputIcon.transform.SetParent(transform, false);

        inputIcon.GetComponent<RectTransform>().anchoredPosition = new(-40, 0);

        return inputIcon;
    }
}