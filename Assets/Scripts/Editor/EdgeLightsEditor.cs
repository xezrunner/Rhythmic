using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EdgeLights))]
public class EdgeLightsEditor : Editor
{
    public void Awake() => script = (EdgeLights)target;
    EdgeLights script;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.LabelField("Edge lights properties", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Color");
        script.Color = EditorGUILayout.ColorField(script.Color);

        EditorGUILayout.LabelField("Glow intensity");
        script.GlowIntenstiy = EditorGUILayout.Slider(script.GlowIntenstiy, 1f, 10f);
    }
}
