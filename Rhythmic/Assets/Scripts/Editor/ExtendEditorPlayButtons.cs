using UnityEditor;
using UnityEngine;
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
        if (prev_scene_path != null && GUILayout.Button(new GUIContent($"restore: {prev_scene_path}", ""))) start_scene(prev_scene_path, false);

        GUILayout.FlexibleSpace();
    }

    static string target_scene_name = null;
    static void start_scene(string scene_name, bool play = true) {
        if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;

        target_scene_name = scene_name;
        should_play = play;

        EditorApplication.update += OnUpdate;
    }

    static bool   should_play = false;
    static string prev_scene_path = null;

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
                prev_scene_path = EditorSceneManager.GetActiveScene().name;

                string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                EditorSceneManager.OpenScene(scenePath);
                if (should_play) EditorApplication.isPlaying = true;
            }
        }
        target_scene_name = null;
    }
}
