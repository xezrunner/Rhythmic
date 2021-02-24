using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldLight))]
public class WorldLightEditor : Editor
{
    WorldLight main;
    void Awake() => main = (WorldLight)target;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.LabelField("Transform", EditorStyles.boldLabel);

        main.Position = EditorGUILayout.Vector3Field("Position", main.Position);

        main.RotationRadius = EditorGUILayout.Slider("RotationRadius", main.RotationRadius, -1, RhythmicGame.Resolution.x * 2);
        main.Rotation = EditorGUILayout.Slider("Rotation", main.Rotation, -360, 360);

        GUILayout.Label($"ActualPosition: {main.ActualPosition}");
    }
}