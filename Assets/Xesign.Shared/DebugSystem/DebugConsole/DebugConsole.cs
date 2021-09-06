using System;

[DebugCom("Prefabs/DebugSystem/DebugConsole")]
public partial class DebugConsole : DebugCom
{
    public static DebugConsole Instance;
    DebugSystem DebugSystem;

    public override void Awake()
    {
        base.Awake();
        Instance = this;
        DebugSystem = DebugSystem.Instance;
    }
    public void Start()
    {
        UI_Start();
        INPUT_Start();
        COMMANDS_Start();
    }

    public static bool ConsoleLog(string text, params object[] args)
    {
        if (Instance) return Instance._ConsoleLog(text, args);
        else throw new Exception("No debug console!");
    }
    public bool _ConsoleLog(string text, params object[] args)
    {
        UI_AddLine(text.Parse(args));
        return true;
    }
    public void Clear() => UI_ClearLines();

    public void Update()
    {
        UPDATE_Input();
        UPDATE_Openness();
        UPDATE_Scroll();
    }
}
