using System.Reflection;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.Runtime;
using Il2CppSystem;
using RedLoader;
using RedLoader.Utils;
using Sons.Gui;
using Sons.Input;
using SUI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Action = System.Action;
using Object = UnityEngine.Object;

namespace SonsSdk;

public class GenericModalDialog : MonoBehaviour
{
    static GenericModalDialog()
    {
        ClassInjector.RegisterTypeInIl2Cpp<GenericModalDialog>();
    }

    private static GenericModalDialog Instance;
    
    private LinkTextGui _titleTextObject;
    private TextMeshProUGUI _contentTextObject;
    
    private LinkTextGui _cancelTextObject;
    private LinkTextGui _option1TextObject;
    private LinkTextGui _option2TextObject;
    private LinkTextGui _option3TextObject;

    private Button _option1Button;
    private Button _option2Button;
    private Button _option3Button;
    private Button _cancelButton;

    private Image _background;

    private TMP_InputField _inputField;
    
    private InputGamepadMenuHelper _gamepadMenuHelper;
    private Canvas _canvas;

    private event Action OnCancel;
    private event Action OnOption1;
    private event Action OnOption2;
    private event Action OnOption3;
    
    private void Option1Clicked()
    {
        OnOption1?.Invoke();
        CloseInternal();
    }
    
    private void Option2Clicked()
    {
        OnOption2?.Invoke();
        CloseInternal();
    }
    
    private void Option3Clicked()
    {
        OnOption3?.Invoke();
        CloseInternal();
    }
    
    private void CancelClicked()
    {
        OnCancel?.Invoke();
        CloseInternal();
    }

    private void SetData(string title, string message)
    {
        _titleTextObject.SetText(title);
        _contentTextObject.text = message;
    }
    
    public void SetOption1(string text, Action action)
    {
        _option1TextObject.SetText(text);
        _option1Button.gameObject.SetActive(true);
        OnOption1 += action;
    }
    
    public void SetOption2(string text, Action action)
    {
        _option2TextObject.SetText(text);
        _option2Button.gameObject.SetActive(true);
        OnOption2 += action;
    }
    
    public void SetOption3(string text, Action action)
    {
        _option3TextObject.SetText(text);
        _option3Button.gameObject.SetActive(true);
        OnOption3 += action;
    }
    
    public void SetCancel(Action action)
    {
        OnCancel += action;
    }

    private void CloseInternal()
    {
        OnCancel = null;
        OnOption1 = null;
        OnOption2 = null;
        OnOption3 = null;

        gameObject.SetActive(false);
    }
    
    private void OpenInternal(string title, string message)
    {
        ResetGui();
        SetData(title, message);
        gameObject.SetActive(true);
        _canvas.overrideSorting = true;
    }

    private void ResetGui()
    {
        _option1Button.gameObject.SetActive(false);
        _option2Button.gameObject.SetActive(false);
        _option3Button.gameObject.SetActive(false);
        _inputField.gameObject.SetActive(false);
        
        _cancelTextObject.SetText("Cancel");
    }

    public static GenericModalDialog ShowDialog(string title, string message)
    {
        Setup();
        Instance.OpenInternal(title, message);
        RLog.Debug("Shown dialog");
        return Instance;
    }

    public static void Close()
    {
        if(!Instance)
            return;
        
        Instance.CloseInternal();
    }

    internal static void Setup()
    {
        if (Instance)
            return;

        var prefab = ModalDialogManager._instance.GameStartingDialog;
        var gui = prefab._gui;

        var newGui = gui.gameObject.Instantiate();
        newGui.SetParent(SUI.SUI.SUIViewport.transform);
        newGui.SetActive(false);

        gui = newGui.GetComponent<DynamicModalDialogGui>();
        
        Instance = newGui.AddComponent<GenericModalDialog>();
        
        Instance._titleTextObject = gui.TitleTextString.GetComponent<LinkTextGui>();
        Destroy(gui.TitleTextString);

        Instance._contentTextObject = gui.ContentTextString.GetComponent<TextMeshProUGUI>();
        Destroy(gui.ContentTextString);
        
        Instance._cancelTextObject = gui.BackTextString.GetComponent<LinkTextGui>();
        Destroy(gui.BackTextString);
        
        Instance._option1TextObject = gui.Option1TextString.GetComponent<LinkTextGui>();
        Destroy(gui.Option1TextString);
        
        Instance._option2TextObject = gui.Option2TextString.GetComponent<LinkTextGui>();
        Destroy(gui.Option2TextString);
        
        Instance._option3TextObject = gui.Option3TextString.GetComponent<LinkTextGui>();
        Destroy(gui.Option3TextString);

        Instance._option1Button = gui.Option1Button;
        Instance._option1Button.onClick.AddListener((UnityAction)Instance.Option1Clicked);
        
        Instance._option2Button = gui.Option2Button;
        Instance._option2Button.onClick.AddListener((UnityAction)Instance.Option2Clicked);
        
        Instance._option3Button = gui.Option3Button;
        Instance._option3Button.onClick.AddListener((UnityAction)Instance.Option3Clicked);

        Instance._cancelButton = gui.BackButton;
        Instance._cancelButton.gameObject.SetActive(true);
        Instance._cancelButton.onClick = new Button.ButtonClickedEvent();
        Instance._cancelButton.onClick.AddListener((UnityAction)Instance.CancelClicked);

        Instance._background = gui.Background;
        
        Instance._inputField = gui.InputField;
        
        Destroy(gui.EventSystem.gameObject);
        Instance._gamepadMenuHelper = gui.GamepadMenuHelper;

        Instance._canvas = newGui.GetComponent<Canvas>();
        
        newGui.transform.localScale = Vector3.one;
        
        var rect = newGui.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;

        Destroy(gui);
    }
}