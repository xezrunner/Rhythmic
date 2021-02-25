using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LightGroup))]
public class LightGroupEditor : Editor
{
    LightGroup main;
    void Awake() => main = (LightGroup)target;

    string Editor_TempName;

    public override void OnInspectorGUI()
    {
        // Name changing: 
        EditorGUILayout.BeginHorizontal();

        if (Editor_TempName == "" || Editor_TempName == null) Editor_TempName = main.Name;
        Editor_TempName = EditorGUILayout.TextField("Name", Editor_TempName);

        if (GUILayout.Button("Change"))
            main.Name = Editor_TempName;

        EditorGUILayout.EndHorizontal();

        //main.Name = EditorGUILayout.TextField("Name", main.Name);
        main.ID = EditorGUILayout.IntField("ID", main.ID);

        base.OnInspectorGUI();
    }
}