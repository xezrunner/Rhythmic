using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityToolbarExtender;

// https://github.com/marijnz/unity-toolbar-extender

[InitializeOnLoad]
public class ExtendEditorPlayButtons {
    static ExtendEditorPlayButtons() {
        ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
    }

    static void OnToolbarGUI() {
        if (GUILayout.Button(new GUIContent("Startup", ""))) start_scene("Startup");

        GUILayout.FlexibleSpace();
    }

    static string target_scene_name = null;
    static void start_scene(string scene_name) {
        if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
        target_scene_name = scene_name;
        EditorApplication.update += OnUpdate;
    }

    static void OnUpdate() {
        if (target_scene_name == null ||
            EditorApplication.isPlaying || EditorApplication.isPaused || EditorApplication.isCompiling ||
            EditorApplication.isPlayingOrWillChangePlaymode) {
            return;
        }
        
        EditorApplication.update -= OnUpdate;

        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
            string[] guids = AssetDatabase.FindAssets("t:scene " + target_scene_name, null);
            if (guids.Length == 0) {
                Debug.LogWarning("Couldn't find scene file");
            } else {
                string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                EditorSceneManager.OpenScene(scenePath);
                EditorApplication.isPlaying = true;
            }
        }
        target_scene_name = null;
    }
}
