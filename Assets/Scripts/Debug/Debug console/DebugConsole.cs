using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static InputHandler;

public enum ConsoleSizeState { Default, Compact, Full }
public enum ConsoleState { Closed, Open }

[DebugComponent(DebugComponentFlag.DebugMenu, DebugComponentType.Prefab_UI, true, -1, "Prefabs/Debug/DebugConsole")]
public partial class DebugConsole : DebugComponent
{
    DebugController DebugController { get { return DebugController.Instance; } }

    public static DebugConsole Instance;
    public static RefDebugComInstance Instances;

    public RectTransform UI_Parent_Trans;
    public RectTransform UI_RectTrans;
    public TMP_Text UI_Text;
    public TMP_InputField Input_Field;

    [NonSerialized] public bool IsOpen;
    [NonSerialized] public ConsoleState State;
    [NonSerialized] public ConsoleSizeState SizeState = ConsoleSizeState.Compact;

    float UI_Canvas_Height { get { return DebugController.UICanvas.rect.height; } } // TODO: Performance!
    [NonSerialized] public float Vertical_Padding = 0f;
    [NonSerialized] public float Compact_Height = 300f;

    [NonSerialized] public float Animation_Speed = 1f;

    Keyboard Keyboard;
    void Awake()
    {
        Instance = this;
        Instances = new RefDebugComInstance(this, gameObject);

        Keyboard = Keyboard.current;
    }
    void Start()
    {
        if (!UI_Text || !Input_Field)
        { Logger.LogMethodE("Console has no UI_Text or Input_Field references!", this, null); return; }

        Logger.LogMethod($"Main canvas height: {UI_Canvas_Height}", this, null);
        target_height = GetHeightForSizeState(ConsoleSizeState.Compact);
        _Close(false);

        RegisterCommonCommands();
    }

    // Animation: | TODO: smoothness, cancellability, fade out DebugUI?
    bool is_animating;
    float anim_t;
    float current_pos, target_pos;
    float current_height, target_height;
    public void Animate(float target, bool anim = true)
    {
        IsOpen = (State == ConsoleState.Open);

        anim_t = (anim) ? 0.0f : 1.0f;

        target_pos = (State == ConsoleState.Closed) ? target : -Vertical_Padding;
        target_height = target - Vertical_Padding * 2f;

        is_animating = true;
    }
    void UPDATE_HandleAnimation()
    {
        if (!is_animating) return;

        current_pos = Mathf.Lerp(current_pos, target_pos, anim_t);
        current_height = Mathf.Lerp(current_height, target_height, anim_t);

        UI_RectTrans.anchoredPosition = new Vector3(UI_RectTrans.anchoredPosition.x, current_pos);
        UI_RectTrans.sizeDelta = new Vector3(UI_RectTrans.sizeDelta.x, current_height);

        if (anim_t < 1.0) anim_t += Time.unscaledDeltaTime * Animation_Speed;
        else is_animating = false;
    }

    // TODO: Ability to change console size
    public float GetHeightForSizeState(ConsoleSizeState size_state)
    {
        switch (size_state)
        {
            default:
            case ConsoleSizeState.Default:
            case ConsoleSizeState.Compact: return Compact_Height;
            case ConsoleSizeState.Full: return UI_Canvas_Height;
        }
    }

    // TODO: Static commands, global control/automation (?)
    public void Toggle()
    {
        if (State == ConsoleState.Closed)
            _Open();
        else _Close();
    }

    public static void Open(bool anim = true, ConsoleSizeState size_state = ConsoleSizeState.Default) => Instance?._Open();
    public static void Close(bool anim = true) => Instance?._Close();
    void _Open(bool anim = true, ConsoleSizeState size_state = ConsoleSizeState.Default)
    {
        State = ConsoleState.Open;
        if (size_state != ConsoleSizeState.Default) target_height = GetHeightForSizeState(size_state);

        AmpPlayerInputHandler.IsActive = false;
        FocusInputField();

        Animate(target_height, anim);
    }
    void _Close(bool anim = true)
    {
        State = ConsoleState.Closed;

        AmpPlayerInputHandler.IsActive = true; // TODO: we want the previous value here? locks?
        UnFocusInputField();

        Animate(target_height, anim);
    }

    // Input field
    string prev_text = "";
    public void OnInputChanged()
    {
        // Make [Escape] not revert the input field
        if (!WasPressed(Keyboard.escapeKey)) prev_text = Input_Field.text;

        // Autocomplete?
    }
    public void OnInputEditingEnd()
    {
        // Make [Escape] not revert the input field
        if (WasPressed(Keyboard.escapeKey)) Input_Field.text = prev_text;
    }

    void FocusInputField()
    {
        //Input_Field.Select();
        Input_Field.ActivateInputField();
    }
    void UnFocusInputField()
    {
        // This calls the OnSubmit() and releated events.
        // In this case, we do not use that event, but it's good to know for future reference.
        Input_Field.DeactivateInputField();
    }

    // Writing to the console
    public static void Write(string text, params object[] args) => Instance?._Write(text, args);
    public static void Log(string text, params object[] args) => Instance?._Log(text, args);

    void _Write(string text, params object[] args)
    {
        string s = "";

        int c = 0, arg_i = 0;
        for (int i = 0; i < text.Length; ++i, ++c)
        {
            if (text[c] == '%')
            {
                if (arg_i >= args.Length) Logger.LogMethodE($"There was no argument at {arg_i} - total count: {args.Length}", this);
                else
                {
                    s += args[arg_i++];
                    continue;
                }
            }

            s += text[c];
        }

        UI_Text.text += s;
    }
    void _Log(string text, params object[] args) => _Write(text + '\n', args);
    // Inconvenient arguments
    public void _LogMethod(string text, object type = null, [CallerMemberName] string methodName = null, params object[] args) => _Write(type + "/" + methodName + ": " + text, args);

    // Console interaction & command processing
    public static bool Process_ReturnOnFoundCommand = true;
    public void Submit()
    {
        string s = Input_Field.text; _Log(s);
        string command = s;
        string[] tokens = new string[0];

        if (s.Contains(' ')) // We have args!
        {
            int command_index = s.IndexOf(' ');
            command = s.Substring(0, command_index);
            ++command_index; // Move index to after the space

            // Get args:
            s = s.Substring(command_index, s.Length - command_index);
            tokens = s.Split(' ');
        }

        // Process commands...
        ProcessCommand(command, tokens);

        Input_Field.text = "";
        FocusInputField();
    }
    public void ProcessCommand(string command, params string[] args)
    {
        bool found = false;

        // Find the command and call it with the args:
        for (int i = 0; i < Commands_Count; ++i)
        {
            ConsoleCommand c = Commands[i];
            if (c.Command == command)
            {
                found = true;
                c.Action(args); // Invoke command action!
                if (Process_ReturnOnFoundCommand) return;
            }
        }

        if (!found) _Log("Command not found: " + "%".AddColor(Colors.Unimportant), command);
    }

    void Update()
    {
        if (is_animating) UPDATE_HandleAnimation();

        if (!IsOpen && WasPressed(Keyboard.digit0Key, Keyboard.backquoteKey))
            _Open();
        else if (IsOpen && WasPressed(Keyboard.escapeKey))
            _Close();

        if (!IsOpen) return;

        if (WasPressed(Keyboard.enterKey, Keyboard.numpadEnterKey))
            Submit();
    }
}