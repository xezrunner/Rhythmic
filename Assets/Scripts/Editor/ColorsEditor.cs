using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Colors))]
class ColorsEditor : Editor
{
    Colors main;
    void Awake() => main = (Colors)target;

    public string output = null;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.LabelField("Tools: ", EditorStyles.boldLabel);

        GUILayout.Label($"Output: {output}");

        if (GUILayout.Button("ConvertToFloatColor()"))
            output = Colors.ConvertToFloatColor(new Color(main.Color.x, main.Color.y, main.Color.z, main.Color.w)).ToString();

        if (GUILayout.Button("ConvertHexToColor()"))
            output = Colors.ConvertHexToColor(main.Input).ToString();

        if (GUILayout.Button("GetColorForCLogType()"))
            output = Colors.GetColorForCLogType(main.LogType).ToString();
    }
}