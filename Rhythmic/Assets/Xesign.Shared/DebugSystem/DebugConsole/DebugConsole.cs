using UnityEngine;

[DebugCom("Prefabs/DebugConsole")]
public partial class DebugConsole : DebugCom
{
    public static DebugConsole Instance;
    DebugSystem DebugSystem;

    public override void Awake()
    {
        base.Awake();
        Instance = this;
        UI_Start();
        DebugSystem = DebugSystem.Instance;
    }
    public void Start()
    {
        INPUT_Start();
        COMMANDS_Start();
    }

    public static bool ConsoleLog(string text, params object[] args)
    {
        if (Instance) return Instance._ConsoleLog(text, args);
        else Debug.LogError("No debug console!".TM("DebugConsole"));

        return true;
    }
    public bool _ConsoleLog(string text, params object[] args)
    {
        UI_AddLine(text.Parse(args));
        if (is_open) UI_ScrollConsole();
        return true;
    }
    public void Clear() => UI_ClearLines();

    public void Update()
    {
        UPDATE_Input();
        UPDATE_Openness();
        if (!is_open) return;

        UPDATE_Scroll();
    }
}
