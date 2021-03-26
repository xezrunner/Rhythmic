using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DebugController))]
public class DebugControllerEditor : Editor
{
    DebugController main;
    void Awake() => main = (DebugController)target;

    DebugComponentFlag state;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // State
        state = main.State;
        state = (DebugComponentFlag)EditorGUILayout.EnumFlagsField("State: ", state);
        if (main.State != state) main.State = state;

        DrawDebugTests();
    }

    bool debugTests_Foldout;
    void DrawDebugTests()
    {
        debugTests_Foldout = EditorGUILayout.BeginFoldoutHeaderGroup(debugTests_Foldout, "Debug tests");

        if (!debugTests_Foldout) { EditorGUILayout.EndFoldoutHeaderGroup(); return; }

        // -----

        if (GUILayout.Button("Set State to None"))
            main.State = DebugComponentFlag.None;

        // -----

        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}
