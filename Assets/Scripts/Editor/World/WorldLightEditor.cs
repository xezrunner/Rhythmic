using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldLight))]
public class WorldLightEditor : Editor
{
    WorldLight main;
    void Awake() => main = (WorldLight)target;

    string Editor_TempName;

    public override void OnInspectorGUI()
    {
        if (!main.Light)
        {
            GUIStyle warningTextStyle = new GUIStyle(EditorStyles.boldLabel);
            warningTextStyle.normal.textColor = Colors.Warning;

            EditorGUILayout.LabelField("Light component not attached.", warningTextStyle);
            EditorGUILayout.LabelField("Attach a light component to change properties.", warningTextStyle);
            EditorGUILayout.Separator();

            base.OnInspectorGUI();
            return;
        }

        DrawLight();

        EditorGUILayout.Separator();

        // Name changing: 
        EditorGUILayout.BeginHorizontal();

        if (Editor_TempName == "" || Editor_TempName == null) Editor_TempName = main.Name;
        Editor_TempName = EditorGUILayout.TextField("Name", Editor_TempName);

        if (GUILayout.Button("Change"))
            main.Name = Editor_TempName;

        EditorGUILayout.EndHorizontal();

        base.OnInspectorGUI();

        DrawAdvanced();
    }

    void DrawLight()
    {
        main.light_foldOut = EditorGUILayout.BeginFoldoutHeaderGroup(main.light_foldOut, "Properties");
        if (!main.light_foldOut) { EditorGUILayout.EndFoldoutHeaderGroup(); return; }

        EditorGUILayout.LabelField("Light: ", EditorStyles.boldLabel);

        main.Color = EditorGUILayout.ColorField("Color", main.Color);
        main.Intensity = EditorGUILayout.Slider("Intensity", main.Intensity, 0, main.MaxIntensity);
        main.Range = EditorGUILayout.Slider("Range", main.Range, 0, main.MaxRadius);
        EditorGUILayout.BeginHorizontal();
        main.Distance = -EditorGUILayout.Slider("Distance", Mathf.Abs(main.Distance), 0, main.MaxDistance); // negative value!
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Rotation transform around circle: ", EditorStyles.boldLabel);

        main.RotationRadius = EditorGUILayout.Slider("Circle radius", main.RotationRadius, -1, RhythmicGame.Resolution.x * 2);
        main.Rotation = EditorGUILayout.Slider("Rotation (pos on circle in angles)", main.Rotation, -360, 360);

        EditorGUILayout.Separator();
        GUILayout.Label($"LightPosition (local): {main.LightPosition}");

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    bool advanced_foldOut;

    void DrawAdvanced()
    {
        advanced_foldOut = EditorGUILayout.BeginFoldoutHeaderGroup(advanced_foldOut, "Editor -> Advanced settings");
        if (!advanced_foldOut) { EditorGUILayout.EndFoldoutHeaderGroup(); return; }

        main.MaxIntensity = EditorGUILayout.FloatField("Max intensity", main.MaxIntensity);
        main.MaxRadius = EditorGUILayout.FloatField("Max radius", main.MaxRadius);
        WorldLight.DefaultRotationRadius = EditorGUILayout.FloatField("Default rotation radius", WorldLight.DefaultRotationRadius);

        EditorGUILayout.Separator();

        main.Gizmos_MainSize = EditorGUILayout.Slider("Gizmos - Main size", main.Gizmos_MainSize, 0, 250);
        RhythmicGame.DebugDrawWorldLights = EditorGUILayout.Toggle("Debug draw world lights (global)", RhythmicGame.DebugDrawWorldLights);


        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}