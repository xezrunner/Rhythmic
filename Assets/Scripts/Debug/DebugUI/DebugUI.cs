using System;
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

public struct DebugUI_SelectionLine // Ambiguity with DebugLine
{
    public DebugUI_SelectionLine(int start, int end, object tag) { startIndex = start; endIndex = end; Tag = tag; Color = Color.white; SelectedColor = Colors.Application; }
    public DebugUI_SelectionLine(int start, int end, object tag, Color color) { startIndex = start; endIndex = end; Tag = tag; Color = color; SelectedColor = Colors.Application; }
    public DebugUI_SelectionLine(int start, int end, object tag, Color color, Color selectedColor) { startIndex = start; endIndex = end; Tag = tag; Color = color; SelectedColor = selectedColor; }

    public int startIndex;
    public int endIndex;
    public object Tag;

    public Color Color;
    public Color SelectedColor;
}

public enum DebugUI_SelectionUpdateMode { Once = 0, ManualFlagging = 1, Always = 2 }

[DebugComponent(DebugComponentFlag.DebugUI, DebugComponentType.Prefab, "Prefabs/Debug/DebugUI")]
public class DebugUI : DebugComponent
{
    public static DebugUI Instance;
    public static GameObject ObjectInstance;

    [Header("Content references")]
    public TextMeshProUGUI framerateText;
    public TextMeshProUGUI debugText;
    public TextMeshProUGUI debugLineText;

    [Header("Control states")]
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

    void Awake()
    {
        Instance = this;
        IsDebugLineOn = _isDebugLineOn;
    }

    /// Debug line

    int bananasCounter = -1;

    // TODO: improve this! Add Logger compatibility!
    // TODO: Colors!
    public static void AddToDebugLine(string text, Color? color = null)
    {
        if (Instance) Instance._AddToDebugLine(text, color);
        else Logger.LogMethod($"DebugUI has no global instance!    -    {text}", "DebugUI", LogTarget.All & ~LogTarget.DebugLine, CLogType.Warning);
    }
    public static void AddToDebugLine(string text, CLogType logType) => AddToDebugLine(text, Colors.GetColorForCLogType(logType));

    void _AddToDebugLine(string text, Color? color = null)
    {
        //if (debugLineText.text.Length == 0) { debugLineText.text = text; return; }
        string s = debugLineText.text;

        int charCount = 0;
        int newlineCount = -1;
        for (int i = 0; i < s.Length; i++, charCount++)
        {
            if (s[i] == '\n')
                newlineCount++;
        }

        if (charCount == 0) // assume that we have one line without a newline
            charCount = s.Length;

        // Apply color if needed
        if (color.HasValue)
            text = text.AddColor(color.Value);

        // TODO: look at these:
        //s = s.Insert(charCount, '\n' + text);
        s = s.Insert(charCount, text + '\n');
        newlineCount++;

        if (newlineCount >= MaxDebugLineCount) // line cleanup! max 4 lines!
            s = s.Remove(0, s.IndexOf('\n') + 1);

        debugLineText.SetText(s);
    }

    /// Main debug text

    string _text;
    public string Text // Hiding!
    {
        get { return _text; }
        set
        {
            _text = value;
            UpdateMainDebugText();
        }
    }

    void UpdateMainDebugText()
    {
        if (!IsDebugPrintOn && !_text.Contains("DEBUG PRINT FREEZE"))
            _text += "\nDEBUG PRINT FREEZE";

        debugText.text = _text;
    }

    /// Debug main loop

    private void ProcessKeys()
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

    float deltaTime;
    void Update()
    {
        ProcessKeys();

        if (!IsDebugUIOn)
            return;

        // update framerate debug
        if (Time.timeScale == 0f)
            return;
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        framerateText.text = string.Format("Framerate: {0} FPS", Mathf.Ceil(fps).ToString());
    }
}
