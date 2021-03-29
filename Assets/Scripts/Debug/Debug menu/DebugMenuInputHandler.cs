using UnityEngine;

public class DebugMenuInputHandler : MonoBehaviour
{
    public static DebugMenuInputHandler Instance;

    public DebugMenu DebugMenu;

    static bool _isActive = true;
    public static bool IsActive
    {
        get
        {
            if (Instance == null) Logger.LogMethodW("requested (get), but instance is null!", "DebugMenuInputHandler");
            return _isActive;
        }
        set
        {
            if (Instance == null) Logger.LogMethodW($"requested (set = {value}), but instance is null!", "DebugMenuInputHandler");
            _isActive = value;
            if (Instance) Instance.gameObject.SetActive(value);
        }
    }

    void Awake()
    {
        Instance = this;
        enabled = _isActive;
    }

    // Functionality:

    void OnOpen() => DebugMenu.SetActive(true);
    void OnClose() => DebugMenu.SetActive(false);
    void OnShowhelp() => Logger.Log("Help!");
    void OnCallmainmenu() => DebugMenu.MainMenu();

    void OnMoveup() => DebugMenu.Entry_Move(DebugMenuEntryDir.Up);
    void OnMovedown() => DebugMenu.Entry_Move(DebugMenuEntryDir.Down);

    void OnEntryfunction() => DebugMenu.PerformSelectedAction();
    void OnIncreasevalue() => DebugMenu.PerformSelectedAction(DebugMenuVarDir.Increase);
    void OnDecreasevalue() => DebugMenu.PerformSelectedAction(DebugMenuVarDir.Decrease);

    void OnHistorybackwards() => DebugMenu.NavigateHistory(DebugMenuHistoryDir.Backwards);
    void OnHistoryforwards() => DebugMenu.NavigateHistory(DebugMenuHistoryDir.Forwards);
}