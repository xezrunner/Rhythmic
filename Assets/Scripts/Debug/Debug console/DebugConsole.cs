using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static InputHandler;

public enum ConsoleSizeState { Default, Compact, Full }
public enum ConsoleState { Closed, Open }

/// TODO:
//  [*] Fix text UI bounds, scrolling
/// 
//  [ ] History + repeat
///
//  [ ] Autocomplete
/// 

[DebugComponent(DebugComponentFlag.DebugMenu, DebugComponentType.Prefab_UI, true, -1, "Prefabs/Debug/DebugConsole")]
public partial class DebugConsole : DebugComponent
{
    DebugController DebugController { get { return DebugController.Instance; } }

    public static DebugConsole Instance;
    public static RefDebugComInstance Instances;

    public RectTransform UI_Parent_Trans;
    public RectTransform UI_RectTrans;

    // Text:
    public ScrollRect UI_ScrollRect;
    public TMP_Text UI_Text;

    // Input field:
    public TMP_InputField Input_Field;
    public TMP_Text UI_Autocomplete;

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
    void UPDATE_Animation()
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

        FocusInputField();

        Animate(target_height, anim);
    }
    void _Close(bool anim = true)
    {
        UnfocusInputField();

        State = ConsoleState.Closed;

        Animate(target_height, anim);
    }

    // Input field
    new string Text = ""; // Hiding: (base) DebugComponent.Text
    public void OnInputChanged()
    {
        if (!IsOpen) // HACK: Since unfocusing the console doesn't seem to want to work, we do this instead.
        {
            Input_Field.text = Text;
            return;
        }

        // Make [Escape] not revert the input field
        if (!WasPressed(Keyboard.escapeKey)) Text = Input_Field.text;

        // Autocomplete:
        if (!autocomplete_enabled || is_autocompleting) return;

        autocomplete_elapsed_since_req = 0f; // Reset autocomplete delay timer
        if (Input_Field.text == "") Autocomplete_WriteEntries(); // clear out
        else autocomplete_requested = true; // Request autocomplete
    }
    public void OnInputEditingEnd()
    {
        // Make [Escape] not revert the input field
        if (WasPressed(Keyboard.escapeKey)) Input_Field.text = Text;
    }

    // Autocomplete:
    bool autocomplete_enabled = true;
    float autocomplete_padding_x = 6f;
    void Autocomplete_WriteEntries(params string[] args)
    {
        UI_Autocomplete.text = "";
        if (args == null || args.Length == 0) return;

        UI_Autocomplete.text += ":: ";

        for (int i = 0; i < args.Length; ++i)
            UI_Autocomplete.text += args[i] + ((i != args.Length - 1) ? "; " : "");

        // Move autocomplete UI to the right of the text
        UI_Autocomplete_UpdateLayout();
    }

    void UI_Autocomplete_UpdateLayout()
    {
        int last_index = Input_Field.textComponent.textInfo.lineInfo[0].lastVisibleCharacterIndex;
        Vector3 c_trans = Input_Field.textComponent.transform.TransformPoint(Input_Field.textComponent.textInfo.characterInfo[last_index].bottomRight);
        UI_Autocomplete.rectTransform.position = new Vector2(c_trans.x + autocomplete_padding_x, UI_Autocomplete.rectTransform.position.y);
    }

    int autocomplete_index = -1;
    List<string> autocomplete_commands;
    bool is_autocompleting = false;

    void Autocomplete_Next()
    {
        if (autocomplete_commands == null || autocomplete_commands.Count == 0) return;

        is_autocompleting = true;

        ++autocomplete_index;
        if (autocomplete_index >= autocomplete_commands.Count) autocomplete_index = 0;

        Input_Field.text = autocomplete_commands[autocomplete_index].ClearColors();
        Input_Field.caretPosition = Input_Field.text.Length;

        // Update autocomplete UI:
        List<string> autocomplete_commands_temp = autocomplete_commands.ToList();
        autocomplete_commands_temp[autocomplete_index] = autocomplete_commands_temp[autocomplete_index].ClearColors().Bold().Underline().AddColor(Colors.Network); // TODO: improve formatting?
        Autocomplete_WriteEntries(autocomplete_commands_temp.ToArray());

        // This is no longer needed if we do the eabove formatting. Leaving it here in case we change our minds.
        //UI_Autocomplete_UpdateLayout();

        is_autocompleting = false;
    }

    bool autocomplete_requested = false;
    float autocomplete_elapsed_since_req = 0f;
    float autocomplete_delay = 300f; // ms
    void UPDATE_Autocomplete()
    {
        if (!autocomplete_requested) return;

        if (autocomplete_elapsed_since_req < autocomplete_delay)
            autocomplete_elapsed_since_req += (Time.unscaledDeltaTime * 1000);
        else // Delay done - do autocomplete:
        {
            List<ConsoleCommand> results = Commands.Where(s => s.Command.Contains(Text)).ToList();
            List<string> s_results = new List<string>();
            int count_of_exact_commands = 0;

            foreach (ConsoleCommand c in results)
            {
                string command = c.Command;
                if (command == Text) ++count_of_exact_commands; // Do not include the actual whole command, but do include similar other ones!

                int index_of_found = command.IndexOf(Text);
                int end_index = index_of_found + Text.Length;
                int end_length = command.Length - (index_of_found + Text.Length);

                string s0 = command.Substring(0, index_of_found);
                string s1 = command.Substring(end_index, end_length);
                string s = (s0 + Text.AddColor(Colors.Application, 1f) + s1);

                s_results.Add(s);
            }

            if (s_results.Count == 1 && count_of_exact_commands == 1) s_results = new List<string>(); // TODO: Performance?

            autocomplete_index = -1;
            autocomplete_commands = s_results;

            Autocomplete_WriteEntries(s_results.ToArray());

            // Reset autocomplete request
            autocomplete_requested = false;
            autocomplete_elapsed_since_req = 0f;
        }
    }

    void FocusInputField()
    {
        Input_Field.ActivateInputField();
        AmpPlayerInputHandler.IsActive = false;
        DebugKeys.IsEnabled = false;
        //Input_Field.Select();
    }
    void UnfocusInputField()
    {
        // This calls the OnSubmit() and releated events.
        // In this case, we do not use that event, but it's good to know for future reference.
        EventSystem.current.SetSelectedGameObject(null); // Unnecessary?
        Input_Field.DeactivateInputField();
        AmpPlayerInputHandler.IsActive = true; // TODO: we want the previous value here? locks?
        DebugKeys.IsEnabled = true;
    }

    // Writing to the console
    public static void Write(string text, params object[] args) => Instance?._Write(text, args);
    public static void Log(string text, params object[] args) => Instance?._Log(text, args);

    public static void Clear() => Instance?._Clear();
    void _Clear() => UI_Text.text = "";

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
        StartCoroutine(ScrollToBottom());
    }
    void _Log(string text, params object[] args) => _Write(text + '\n', args);
    // Inconvenient arguments
    public void _LogMethod(string text, object type = null, [CallerMemberName] string methodName = null, params object[] args) => _Write(type + "/" + methodName + ": " + text, args);

    IEnumerator ScrollToBottom() // HACK: You have to wait for the end of the current frame to be able to scroll to the bottom.
    {
        yield return new WaitForEndOfFrame();
        UI_ScrollRect.verticalNormalizedPosition = 0f;
    }

    // Console interaction & command processing
    public static bool ReturnOnFoundCommand = true;
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

                // TODO: add function name?
                bool is_exclusive_help = (args.Contains("--help") || (!c.is_action_empty && args.Length == 0));
                if (is_exclusive_help && command.StartsWith("set_")) is_exclusive_help = false; // TEMP / HACK!

                if (is_exclusive_help || args.Length == 0)
                    //Log($"{c.Command.AddColor(Colors.Application)}: {c.HelpText}".AddColor(Colors.Unimportant));
                    Log($"{c.HelpText}".AddColor(Colors.Unimportant));

                if (!is_exclusive_help) c.Invoke(args); // Invoke command action!
                if (ReturnOnFoundCommand) return;
            }
        }

        if (!found) _Log("Command not found: " + "%".AddColor(Colors.Unimportant), command);
    }
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

            // Spaces shouldn't count as args
            int space_counter = 0;
            for (int i = 0; i < tokens.Length; ++i)
                if (tokens[i] == " " || tokens[i] == "")
                { tokens[i] = null; ++space_counter; }

            if (space_counter == tokens.Length) tokens = new string[0];
        }

        Input_Field.text = "";
        FocusInputField();

        // Process commands...
        ProcessCommand(command, tokens);

    }

    void Update()
    {
        UPDATE_Animation();
        UPDATE_Autocomplete();

        if (!IsOpen && WasPressed(Keyboard.digit0Key, Keyboard.backquoteKey))
            _Open();
        else if (IsOpen && WasPressed(Keyboard.escapeKey))
            _Close();

        if (!IsOpen) return;

        if (WasPressed(Keyboard.enterKey, Keyboard.numpadEnterKey))
            Submit();
        if (WasPressed(Keyboard.tabKey))
            Autocomplete_Next();
    }
}