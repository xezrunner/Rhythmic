using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DebugController))]
public class DebugControllerEditor : Editor
{
    DebugController main;
    void Awake() => main = (DebugController)target;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // State
        main.State = (DebugControllerState)EditorGUILayout.EnumFlagsField("State: ", main.State);

        DrawDebugTests();
    }

    bool debugTests_Foldout;
    void DrawDebugTests()
    {
        debugTests_Foldout = EditorGUILayout.BeginFoldoutHeaderGroup(debugTests_Foldout, "Debug tests");

        if (!debugTests_Foldout) { EditorGUILayout.EndFoldoutHeaderGroup(); return; }

        // -----

        if (GUILayout.Button("Set State to None"))
            main.State = DebugControllerState.None;

        // -----

        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}
