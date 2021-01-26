using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EdgeLights))]
public class EdgeLightsEditor : Editor
{
    public void Awake() => script = (EdgeLights)target;
    EdgeLights script;

    Color color;
    float glowIntensity;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (script.MeshRenderer == null || script.MeshFilter == null)
        {
            EditorGUILayout.LabelField("No MeshRenderer or MeshFilter is assigned!");
            return;
        }
        if (!Application.isPlaying) return;

        EditorGUILayout.LabelField("Edge lights properties", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Color");
        color = EditorGUILayout.ColorField(color);

        EditorGUILayout.LabelField("Glow intensity");
        glowIntensity = EditorGUILayout.Slider(glowIntensity, 1f, 10f);

        if (GUILayout.Button("Update color & glow!"))
        {
            script.Color = color;
            script.GlowIntenstiy = glowIntensity;
        }
    }
}
