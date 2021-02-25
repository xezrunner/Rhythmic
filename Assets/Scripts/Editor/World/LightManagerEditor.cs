using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LightManager))]
public class LightManagerEditor : Editor
{
    LightManager main;
    void Awake() => main = (LightManager)target;

    WorldLight currentLight;
    WorldLight foundLight;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        DrawTools();
        DrawFind();
        DrawLightList();
        DrawCurrentLight();
    }

    void DrawTools()
    {
        if (Application.isEditor && !Application.isPlaying) return;

        main.Editor_ToolsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(main.Editor_ToolsFoldout, "Tools");
        if (!main.Editor_ToolsFoldout) { EditorGUILayout.EndFoldoutHeaderGroup(); return; }

        if (GUILayout.Button("Fade in all lights"))
            main.AnimateIntensities(main.LightGroups, -1);
        if (GUILayout.Button("Fade out all lights"))
            main.AnimateIntensities(main.LightGroups, 0);

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    bool find_foldOut;
    void DrawFind()
    {
        find_foldOut = EditorGUILayout.BeginFoldoutHeaderGroup(find_foldOut, "Manual search");
        if (!find_foldOut) { EditorGUILayout.EndFoldoutHeaderGroup(); return; }

        main.Editor_LightToFind = EditorGUILayout.TextField("Light to find", main.Editor_LightToFind);
        if (GUILayout.Button("Find"))
            foundLight = main.FindLight(main.Editor_LightToFind);

        if (foundLight)
        {
            EditorGUILayout.LabelField("Found: ", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Name: {foundLight.name}");
            EditorGUILayout.LabelField($"ID: {foundLight.ID}");
            EditorGUILayout.LabelField($"Color: {foundLight.Color}");
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    void DrawLightList()
    {
        if (Application.isEditor && !Application.isPlaying) return;

        main.Editor_LightListFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(main.Editor_LightListFoldout, "Current lights");
        if (!main.Editor_LightListFoldout) { EditorGUILayout.EndFoldoutHeaderGroup(); return; }

        foreach (var group in main.LightGroups)
        {
            EditorGUILayout.LabelField($"{group.Name}:", EditorStyles.boldLabel);

            foreach (WorldLight light in group.Lights)
            {
                EditorGUI.BeginDisabledGroup(currentLight == light);
                if (GUILayout.Button($"{light.Name} [{light.ID}]"))
                    currentLight = light;
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.Separator();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    bool light_foldOut = true;
    void DrawCurrentLight()
    {
        if (!currentLight) return;
        if (Application.isEditor && !Application.isPlaying) return;

        light_foldOut = EditorGUILayout.BeginFoldoutHeaderGroup(light_foldOut, "Selected light properties");
        if (!light_foldOut) { EditorGUILayout.EndFoldoutHeaderGroup(); return; }

        EditorGUILayout.LabelField($"Selected light: {currentLight.Name} [{currentLight.ID}]", EditorStyles.boldLabel);

        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}