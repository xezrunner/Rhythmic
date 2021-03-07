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

        EditorGUILayout.LabelField("Tools:", EditorStyles.boldLabel);

        if (GUILayout.Button("Refresh tunnel"))
            main.InitTunnel();

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Object tools: ", EditorStyles.boldLabel);

        if (GUILayout.Button("Add object at rotation"))
            main.AddObject();

        if (GUILayout.Button("Refresh last object"))
            main.RefreshObject();

        EditorGUILayout.Separator();
        if (GUILayout.Button("Add 6 ID items"))
            main.Add6Objects();

        if (GUILayout.Button("Delete all"))
            main.RemoveAll();
    }
}