using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugStats : MonoBehaviour
{
    [SerializeField] TMP_Text ui_text;

    StringBuilder builder = new StringBuilder(4096);

    void UPDATE_Text() {
        builder.Append($"Framerate: {Mathf.CeilToInt(framerate)} {(!Application.isEditor && QualitySettings.vSyncCount > 0 ? "(VSYNC)" : null)}");
#if UNITY_EDITOR
        builder.Append($"| EDITOR");
#endif
        builder.Append("\n\n");

        var active_scene = SceneManager.GetActiveScene();
        builder.AppendLine($"Active scene: {active_scene.name}");
        builder.AppendLine($"Loaded scenes:");
        for (int i = 0; i < SceneManager.sceneCount; ++i) {
            var scene = SceneManager.GetSceneAt(i);
            if (scene == active_scene) continue;
            builder.AppendLine($" -{scene.name}");
        }
        builder.AppendLine();

        builder.AppendLine("Printing test array with StringBuilder:");
        string[] test_array = { "test1", "test2", "test3" };
        builder.Append(" (");
        builder.AppendJoin("; ", test_array);
        builder.Append(')');
        builder.AppendLine();

        ui_text.SetText(builder.ToString());
        builder.Clear();
    }

    float framerate_dt;
    float framerate;
    void Update() {
        framerate_dt += (Time.unscaledDeltaTime - framerate_dt) * 0.1f;
        framerate = 1.0f / framerate_dt;
        UPDATE_Text();
    }
}
