using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LoggerTesterCom))]
class LoggerTesterComEditor : Editor
{
    LoggerTesterCom main;
    void Awake() => main = (LoggerTesterCom)target;

    string separatorChar = ",";
    bool printIndex = true;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Label("Actions: ");

        if (GUILayout.Button("Log!"))
            main.SendText();

        if (GUILayout.Button("Log the object!"))
            main.SendObject(printIndex, separatorChar[0]);

        // ----- Object logging props ----- //
        GUILayout.BeginHorizontal();

        printIndex = GUILayout.Toggle(printIndex, "printIndex");
        GUILayout.Label("separatorChar: ");
        separatorChar = GUILayout.TextField(separatorChar);

        GUILayout.EndHorizontal();
        // ----- Object logging props ----- //

        GUILayout.Label("Utilities: ");

        if (GUILayout.Button("Start CServer"))
            ConsoleServer.StartConsoleServer();
        if (GUILayout.Button("Stop CServer"))
            ConsoleServer.StopConsoleServer();
    }
}