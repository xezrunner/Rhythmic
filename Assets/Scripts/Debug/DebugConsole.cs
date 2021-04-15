using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static InputHandler;

public enum ConsoleSizeState { Default, Compact, Full }
public enum ConsoleState { Closed, Open }

[DebugComponent(DebugComponentFlag.DebugMenu, DebugComponentType.Prefab_UI, true, -1, "Prefabs/Debug/DebugConsole")]
public class DebugConsole : DebugComponent
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
    [NonSerialized] public float Vertical_Padding = 4f;
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
        Close(false);
    }

    // Animation: | TODO: smoothness, cancellability, fade out DebugUI?
    float current_pos, target_pos;
    float current_height, target_height;
    public void Animate(float target, bool anim = true) => StartCoroutine(_Animate(target, anim));
    IEnumerator _Animate(float target, bool anim = true)
    {
        IsOpen = (State == ConsoleState.Open);
        target_pos = (State == ConsoleState.Closed) ? target : -Vertical_Padding;
        target_height = target - Vertical_Padding * 2f;

        float t = (anim ? 0.0f : 1.0f);

        do
        {
            current_pos = Mathf.Lerp(current_pos, target_pos, t);
            current_height = Mathf.Lerp(current_height, target_height, t);

            UI_RectTrans.anchoredPosition = new Vector3(UI_RectTrans.anchoredPosition.x, current_pos);
            UI_RectTrans.sizeDelta = new Vector3(UI_RectTrans.sizeDelta.x, current_height);

            t += Time.unscaledDeltaTime * Animation_Speed;
            yield return null;
        } while (t < 1.0f);
    }

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

    public void Toggle()
    {
        if (State == ConsoleState.Closed)
            Open();
        else Close();
    }

    public void Open(bool anim = true, ConsoleSizeState size_state = ConsoleSizeState.Default)
    {
        State = ConsoleState.Open;
        if (size_state != ConsoleSizeState.Default) target_height = GetHeightForSizeState(size_state);

        AmpPlayerInputHandler.IsActive = false;
        EventSystem.current.SetSelectedGameObject(Input_Field.gameObject);
        Input_Field.ActivateInputField();

        Animate(target_height, anim);
    }
    public void Close(bool anim = true)
    {
        State = ConsoleState.Closed;
        AmpPlayerInputHandler.IsActive = true;

        EventSystem.current.SetSelectedGameObject(null);
        Input_Field.DeactivateInputField();

        Animate(target_height, anim);
    }

    public void Write(string text, params object[] args)
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
    public void Log(string text, params object[] args) => Write(text + '\n', args);

    public void OnInputChanged()
    {
        // Autocomplete?
    }

    public void OnSubmit()
    {
        string s = Input_Field.text;
        Log("User input: %", s);
        // Process commands...

        Input_Field.text = "";
    }

    void Update()
    {
        if (WasPressed(Keyboard.digit0Key, Keyboard.backquoteKey))
            Toggle();
    }
}