using System.Collections;
using Pathfinding.Util;
using RedLoader;
using Sons.Ai.Vail;
using Sons.Gui.Input;
using SonsSdk;
using TheForest.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Color = System.Drawing.Color;
using InputSystem = Sons.Input.InputSystem;
using Object = UnityEngine.Object;

namespace SUI;

public class SKeybindOptions : SUiElement<SKeybindOptions>
{
    public RebindingInputOptionGui RebindingInputOptionGui;
    public DynamicInputIcon DynamicInputIcon;
    public Button RebindButton;
    
    private EventSystem _eventSystem;
    private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;
    private KeybindConfigEntry _keybindConfig;
    private GameObject _instance;

    public SKeybindOptions(GameObject root) : base(root)
    {
        RebindingInputOptionGui = root.GetComponent<RebindingInputOptionGui>();
        DynamicInputIcon = root.GetComponent<DynamicInputIcon>();
        RebindButton = root.FindGet<Button>("InputSelectionPanel/Panel/InputButton01");
        TextObject = root.FindGet<TextMeshProUGUI>("LabelPanel/Label");
        
        RebindButton.onClick.AddListener((UnityAction)StartRebindOperation);
        
        root.SetActive(true);
    }
    
    private void StartRebindOperation()
    {
        if(_keybindConfig == null)
        {
            return;
        }

        void CancelCallback(InputActionRebindingExtensions.RebindingOperation op)
        {
            _rebindingOperation.Dispose();
            _rebindingOperation = null;
            OnBindingCancelled();
        }
        
        void ChangeBinding(InputActionRebindingExtensions.RebindingOperation op)
        {
            _rebindingOperation.Dispose();
            _rebindingOperation = null;
            OnBindingChanged();
        }
        
        OnBindingStarted();
        _rebindingOperation = new InputActionRebindingExtensions.RebindingOperation();
        var action = _keybindConfig.GetAction();
        action.Disable();
        _rebindingOperation.WithAction(action).WithTimeout(5f).WithMatchingEventsBeingSuppressed()
            .WithExpectedControlType("Button")
            .OnApplyBinding((Il2CppSystem.Action<InputActionRebindingExtensions.RebindingOperation, string>)OnApplyBinding)
            .OnCancel((Il2CppSystem.Action<InputActionRebindingExtensions.RebindingOperation>)CancelCallback)
            .OnComplete((Il2CppSystem.Action<InputActionRebindingExtensions.RebindingOperation>)ChangeBinding)
            .Start();
    }

    private void OnApplyBinding(InputActionRebindingExtensions.RebindingOperation op, string path)
    {
        var action = _keybindConfig.GetAction();
        action.ApplyBindingOverride(path);

        RLog.Msg(Color.Orange, $"Applied binding override to {action.name} [Path: {path}");
    }

    private void OnBindingStarted()
    {
        RebindingInputOptionGui.SetBindingMode(true);
        DisableEventSystem();
    }

    private void OnBindingChanged()
    {
        RebindingInputOptionGui.SetBindingMode(false);
        EnableEventSystemWithDelay().RunCoro();
        CreateNewInstance();

        _keybindConfig?.GetKeybind().SaveToConfig();

        _keybindConfig.GetAction().Enable();
        
        // == HACK ==
        _eventSystem.gameObject.SetActive(true);
        InputSystem.SetState(InputState.InputBinding, false);
    }

    private void OnBindingCancelled()
    {
        RebindingInputOptionGui.SetBindingMode(false);
        EnableEventSystemWithDelay().RunCoro();
        
        _keybindConfig.GetAction().Enable();
        
        // == HACK ==
        _eventSystem.gameObject.SetActive(true);
        InputSystem.SetState(InputState.InputBinding, false);
    }

    private void DisableEventSystem()
    {
        _eventSystem = EventSystem.current;
        
        _eventSystem.gameObject.SetActive(false);
        InputSystem.SetState(InputState.InputBinding, true);
    }
    
    private IEnumerator EnableEventSystemWithDelay()
    {
        yield return new WaitForSeconds(0.55f);
        _eventSystem.gameObject.SetActive(true);
        InputSystem.SetState(InputState.InputBinding, false);
        RLog.Msg(Color.Orange, "Event system activated");
    }

    private GameObject CreateNewInstance()
    {
        var defaultInstance = DynamicInputIcon._defaultInstance;
        
        GameObject keyPrefab = InputIconManager.Get(_keybindConfig.GetAction(), out var displayName, out var isDefaultPrefab);
        if (keyPrefab == null)
        {
            return null;
        }
        
        DynamicInputIcon._activeInstance.TryDestroy();
        _instance.TryDestroy();
        
        _instance = Object.Instantiate(keyPrefab, defaultInstance.transform.parent);
        if (defaultInstance != null)
        {
            RectTransform rect = _instance.GetComponent<RectTransform>();
            CopyRectTransform(defaultInstance.GetComponent<RectTransform>(), rect);
        }
        
        _instance.name = keyPrefab.name;
        int siblingIndex = (defaultInstance ? defaultInstance.transform.GetSiblingIndex() : 0);
        _instance.transform.SetSiblingIndex(siblingIndex);
        
        if (isDefaultPrefab)
        {
            TMP_Text tmpText = _instance.GetComponentInChildren<TMP_Text>(true);
            if (tmpText != null)
            {
                tmpText.text = displayName;
            }
        }
        return _instance;
    }
    
    private static void CopyRectTransform(RectTransform fromTransform, RectTransform toTransform)
    {
        toTransform.localScale = fromTransform.localScale;
        toTransform.localRotation = fromTransform.localRotation;
        toTransform.localPosition = fromTransform.localPosition;
        toTransform.anchorMin = fromTransform.anchorMin;
        toTransform.anchorMax = fromTransform.anchorMax;
        toTransform.anchoredPosition = fromTransform.anchoredPosition;
        toTransform.sizeDelta = fromTransform.sizeDelta;
        toTransform.pivot = fromTransform.pivot;
        toTransform.offsetMin = fromTransform.offsetMin;
        toTransform.offsetMax = fromTransform.offsetMax;
    }
    
    public void RevertToDefault()
    {
        if (_keybindConfig == null)
        {
            return;
        }
        
        _keybindConfig.GetKeybind().RevertToDefault();
        CreateNewInstance();
    }

    public SKeybindOptions Config(KeybindConfigEntry config)
    {
        _keybindConfig = config;
        CreateNewInstance();
        return this;
    }

    /// <summary>
    /// Forces a total height for the keybind container
    /// </summary>
    /// <param name="height"></param>
    /// <returns></returns>
    public SKeybindOptions BindingInputHeight(float height)
    {
        var layout = Root.FindGet<LayoutElement>("InputSelectionPanel/Panel/InputButton01/");
        layout.minHeight = -1;
        layout.preferredHeight = -1;
        layout = Root.FindGet<LayoutElement>("InputSelectionPanel");
        layout.minHeight = -1;
        layout.preferredHeight = height;
        var rect = Root.FindGet<RectTransform>("InputSelectionPanel/Panel/InputButton01/InputIcon");
        rect.sizeDelta = new(height-10, height-10);
        return this;
    }
}