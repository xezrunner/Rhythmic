using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DebugComponent(DebugComponentFlag.DebugMenu, DebugComponentType.Component, true)]
public class DebugMenu : DebugComponent
{
    DebugUI DebugUI;

    TextMeshProUGUI debugmenuText;
    public Image debugmenuArrow;

    public static RefDebugComInstance Instance;

    Dictionary<string, DebugMenuComponent> Instances = new Dictionary<string, DebugMenuComponent>();
    DebugMenuComponent ActiveComponent = null;

    bool _isActive;
    public bool IsActive { get { return _isActive; } }
    public void SetActive(bool value)
    {
        _isActive = value;
        debugmenuText.gameObject.SetActive(value);
        if (value) MainMenu();
    }

    void Awake()
    {
        Instance = new RefDebugComInstance(this);

        DebugUI = DebugUI.Instance;
        debugmenuText = DebugUI.debugmenuText;
        debugmenuArrow = DebugUI.debugmenuArrow;
    }
    void Start()
    {
        if (!DebugUI)
        { Logger.LogMethodW("DebugUI instance doesn't seem to exist!", this); return; }
    }

    // There's always one component that's considered as the Main menu.
    // It can be switched to a different one, but there has to be one.
    public static void MainMenu()
    {
        DebugMenu debugMenu = (DebugMenu)Instance.Component;
        debugMenu.SwitchToComponent(typeof(DebugMenus.MainMenu));
    }

    // Component switching & handling:
    public void SwitchToComponent(Type type)
    {
        DebugMenuComponent instance = (DebugMenuComponent)type.GetField("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null);

        if (instance == null)
        {
            DebugMenuComponent com = (DebugMenuComponent)Activator.CreateInstance(type);
            com.Init();
            if (!Instances.ContainsKey(type.Name)) Instances.Add(type.Name, com);
            instance = com;

            Logger.LogMethodW($"Component {type.Name} did not exist, so we instanced one. (total: {Instances.Count})");
        }
        //{ Logger.LogMethodW($"Cannot switch to component {type.Name} - not available!", this); return; }

        SwitchToComponent(instance);
    }
    public void SwitchToComponent(DebugMenuComponent com = null)
    {
        if (com != null) ActiveComponent = com;
        Handle_ActiveComponent();
    }
    void Handle_ActiveComponent()
    {
        if (ActiveComponent == null)
        { debugmenuText.text = ""; return; }

        if (_isActive)
            debugmenuText.text = ActiveComponent.Main();
    }

    // Input processing:
    void ProcessKeys()
    {
        // Enable & disable | F1: ON ; F2: OFF
        if (Keyboard.current.f1Key.wasPressedThisFrame) SetActive(true);
        else if (Keyboard.current.f2Key.wasPressedThisFrame) SetActive(false);

        if (!_isActive) return;

        // Move through entries:
        // U: Move down ; O: Move up
        // Space: enter ; 1-2: Change value of variable entries
    }

    // Navigation:
    

    float update_ElapsedMs;
    void Update()
    {
        if (ActiveComponent != null && ActiveComponent.Attribute.UpdateFrequencyInMs != -1)
        {
            update_ElapsedMs += Time.unscaledDeltaTime * 1000;
            if (ActiveComponent.Attribute.UpdateFrequencyInMs == 0 || update_ElapsedMs >= ActiveComponent.Attribute.UpdateFrequencyInMs)
                Handle_ActiveComponent();
        }

        ProcessKeys();
    }
}