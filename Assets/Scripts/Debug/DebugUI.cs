using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public enum DebugUILevel
{
    None = 0,
    Framerate = 1 << 0,
    DebugLine = 1 << 1,
    ShortStats = 1 << 2,
    ShortShortStats = 1 << 3,
    Stats = 1 << 4,
    InternalStats = 1 << 5,

    Default = (Framerate | DebugLine | ShortStats),
    Full = (Framerate | DebugLine | Stats),
    Internal = (Stats | InternalStats)
}
public enum ComponentDebugLevel
{
    None = 0,
    PlayerStats = 1 << 0,
    SongStats = 1 << 1,
    TracksStats = 1 << 2,
    TracksDetailedStats = 1 << 3,
    AudioStats = 1 << 4,
    ClockStats = 1 << 5,
    LightingStats = 1 << 6,
    AnimationStats = 1 << 7,
    Misc = 1 << 8,

    Default = (SongStats | PlayerStats | TracksStats | ClockStats | Misc),
    Audio = (SongStats | TracksDetailedStats | AudioStats | ClockStats),
    Lighting = (PlayerStats | LightingStats)
}

[DebugComponent(DebugComponentFlag.DebugUI, DebugComponentType.Prefab, "Prefabs/Debug/DebugUI")]
public class DebugUI : DebugComponent
{
    public static DebugUI Instance;
    public static RefDebugComInstance Instances;

    [Header("Content references")]
    public TextMeshProUGUI framerateText;
    public TextMeshProUGUI debugText;
    public TextMeshProUGUI debugLineText;

    [Header("Properties")]
    public float SelfDebugOpacity = 0.8f;

    [Header("Control states")]
    [NonSerialized] public bool IsSelfDebug = false;
    public bool AlwaysUpdate = false;

    public bool IsDebugUIOn = true;
    public bool IsDebugPrintOn = true;
    public bool _isDebugLineOn = true;
    public bool IsDebugLineOn
    {
        get { return debugLineText.gameObject.activeSelf; }
        set { debugLineText.gameObject.SetActive(value); }
    }

    [Header("Debug Line")]
    public int MaxDebugLineCount = 4;
    public static int DebugLine_LineTimeoutMs = 5000;

    void Awake()
    {
        Instance = this;
        Instances = new RefDebugComInstance(this, gameObject);
        IsDebugLineOn = _isDebugLineOn;
    }

    /// Interface switching

    DebugComponent ActiveComponent;

    // Switches to a different debug component interface.
    public void SwitchToComponent(Type type)
    {
        RefDebugComInstance instance = (RefDebugComInstance)type.GetField("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null);

        if (instance.Component == null)
        { Logger.LogWarning($"Cannot switch to component {type.Name} - not available!"); return; }

        SwitchToComponent(instance.Component);
    }
    public void SwitchToComponent(DebugComponent com)
    {
        if (ActiveComponent == com) return;
        if (com == null)
        { Logger.LogWarning($"Component {com.GetType().Name} is not available!"); return; }

        //if (com.Attribute.DebugFlag != DebugComponentFlag.DebugUI || com.Attribute.DebugFlag != DebugComponentFlag.DebugInterfaces || com.Attribute.DebugFlag != DebugComponentFlag.DebugMenu)
        //{ Logger.LogMethod($"Component {com.Name} has the debug flag {com.Attribute.DebugFlag}, which isn't allowed in DebugUI.", this, CLogType.Error); return; }

        ActiveComponent = com;
        HandleActiveComponentText(true); // Grab text forcefully upon switching
    }
    public void SwitchToComponent() // Empty active component
    {
        ActiveComponent = null;
        MainText = "";
    }

    /// Debug line

    int bananasCounter = -1;

    // TODO: improve this! Add Logger compatibility!
    // TODO: Colors!
    public static void AddToDebugLine(string text, Color? color = null)
    {
        // TODO!!!: Flags bit hacks don't seem to work here when in editor & not playing!
        if (Instance) Instance._AddToDebugLine(text, color);
        else Logger.LogMethod($"DebugUI has no global instance!    -    {text}", CLogType.Warning, logTarget: LogTarget.UnityAndConsole, "DebugUI");
    }
    public static void AddToDebugLine(string text, CLogType logType) => AddToDebugLine(text, Colors.GetColorForCLogType(logType));
    void _AddToDebugLine(string text, Color? color = null)
    {
        debugLine_ElapsedTime = 0; // Reset timeout

        string s = debugLineText.text;

        // Apply color if needed
        if (color.HasValue)
            text = text.AddColor(color.Value);

        s += text + '\n';

        // line cleanup!
        s = s.MaxLines(MaxDebugLineCount);

        debugLineText.SetText(s);
    }

    // TODO: fade effects?
    void HandleDebugLineTimeout()
    {
        if (debugLineText.text.Length == 0)
            return;

        // Keep track of elapsed ms for debug line
        debugLine_ElapsedTime += Time.unscaledDeltaTime * 1000;

        if (debugLine_ElapsedTime > DebugLine_LineTimeoutMs)
        {
            debugLine_ElapsedTime = 0;
            string[] lines = debugLineText.text.Split('\n');
            string[] newLines = new string[lines.Length - 1];
            for (int i = 1; i < lines.Length; i++)
                newLines[i - 1] = lines[i];

            debugLineText.text = string.Join("\n", newLines);
        }
    }

    /// Main debug text

    void HandleActiveComponentText(bool force = false)
    {
        if (!ActiveComponent) return;

        float updateFreq = ActiveComponent.Attribute.UpdateFrequencyInMs;
        if (force || updateFreq != -1)
        {
            if (updateFreq != 0)
            {
                if (mainText_ElapsedTime > updateFreq)
                    mainText_ElapsedTime = 0;
                else return; // Not enough time has passed!
            }

            // If com attr TextMode is Clear, remove component text before calling the component!
            if (ActiveComponent.Attribute.TextMode == DebugComTextMode.Clear) ActiveComponent.ClearText();
            ActiveComponent.UI_Main();

            if (ActiveComponent.Text == "")
                Logger.LogMethod($"Component {ActiveComponent.Name.AddColor(Colors.Application)} is not an UI component.", CLogType.Warning, this);
            else
                MainText = ActiveComponent.Text;
        }
    }

    string _mainText;
    public string MainText // This is the main debug UI text, not to be confused with DebugComponent.Text!
    {
        get { return _mainText; }
        set
        {
            _mainText = value;
            UpdateMainDebugText();
        }
    }

    string SelfDebug()
    {
        string s = $"DebugUI SELF DEBUG:\n".AddColor(1, 1, 1) +
                   $"Active component: {(ActiveComponent ? ActiveComponent.Name.AddColor(Colors.Application, SelfDebugOpacity) : "None")}\n" +
                   $"Elapsed time since last update: {mainText_ElapsedTime} ms {((ActiveComponent && ActiveComponent.Attribute.UpdateFrequencyInMs != -1) ? $"  Active com update freq: {ActiveComponent.Attribute.UpdateFrequencyInMs}" : "")}\n" +
                   $"" +
                   $"\n";
        s = s.AddColor(1, 1, 1, SelfDebugOpacity);

        return s;
    }

    void UpdateMainDebugText()
    {
        string s = IsSelfDebug ? SelfDebug() : "";

        if (!IsDebugPrintOn && ActiveComponent)
            s += "DEBUG PRINT FREEZE\n\n";

        debugText.text = s + _mainText;
    }

    /// Debug main loop

    void ProcessKeys()
    {
        // Debug control
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F3))
        {
            IsDebugUIOn = !IsDebugUIOn;
            //Main.SetActive(IsDebugOn);
        }

        if (!IsDebugUIOn)
            return;

        // ----- DEBUG LOOP ----- //

        if (Input.GetKeyDown(KeyCode.L))
            AddToDebugLine($"bananas! {bananasCounter++}");

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F3)) // freeze printing
        {
            IsDebugPrintOn = !IsDebugPrintOn;
            UpdateMainDebugText();
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F4)) // disable debug print UI
        {
            IsDebugPrintOn = !IsDebugPrintOn;
            //Main.SetActive(IsDebugPrintOn); // TODO: fix!
        }
        if (Input.GetKeyDown(KeyCode.F5))
        { }
    }

    float debugLine_ElapsedTime;
    float mainText_ElapsedTime;
    float FPS_deltaTime;
    void Update()
    {
        ProcessKeys();

        if (!IsDebugUIOn)
            return;

        // MAIN DEBUG LOOP:

        HandleDebugLineTimeout();

        // Keep track of elapsed ms for update frequency
        mainText_ElapsedTime += Time.unscaledDeltaTime * 1000;
        HandleActiveComponentText(); // Get active component text!

        // Update self text manually if needed
        if (IsSelfDebug || AlwaysUpdate)
            UpdateMainDebugText();

        // update framerate debug
        if (Time.timeScale == 0f)
            return;
        FPS_deltaTime += (Time.unscaledDeltaTime - FPS_deltaTime) * 0.1f;
        float fps = 1.0f / FPS_deltaTime;
        framerateText.text = string.Format("Framerate: {0} FPS", Mathf.Ceil(fps).ToString());
    }
}
