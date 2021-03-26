using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(_TunnelTest))]
class _TunnelTestEditor : Editor
{
    _TunnelTest main;
    void Awake() => main = (_TunnelTest)target;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!Application.isPlaying) return;

        if (GUILayout.Button("Refresh tunnel"))
            main.Start();

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Object tools: ", EditorStyles.boldLabel);

        if (GUILayout.Button("Add object at rotation"))
            main.AddObject();

        if (GUILayout.Button("Refresh last object"))
            main.RefreshObject();

        EditorGUILayout.Separator();
        if (GUILayout.Button("Add 6 ID items"))
            for (int i = 0; i < 6; i++)
                main.AddObject(i);

        if (GUILayout.Button("Delete all"))
            main.RemoveAll();
    }
}