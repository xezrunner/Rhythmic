using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Colors))]
class ColorsEditor : Editor
{
    Colors main;
    void Awake() => main = (Colors)target;

    public Color? output = null;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Label($"Output: {output}");

        if (GUILayout.Button("ConvertToFloatColor()"))
            output = Colors.ConvertToFloatColor(new Color(main.Color.x, main.Color.y, main.Color.z, main.Color.w));

        if (GUILayout.Button("ConvertHexToColor()"))
            output = Colors.ConvertHexToColor(main.Input);
    }
}