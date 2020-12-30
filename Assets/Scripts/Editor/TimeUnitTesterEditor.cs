using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TimeUnitTester))]
public class TimeUnitTesterEditor : Editor
{
    TimeUnitTester script;
    SongController SongController { get { return SongController.Instance; } }

    void Awake() => script = (TimeUnitTester)target;

    public float input { get { return script.Input; } }
    public float value = 0f;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Label($"Answer: {value}");

        GUILayout.Label("Ticks ->");

        if (GUILayout.Button("TickToSec"))
            value = SongController.TickToSec((int)input);
        if (GUILayout.Button("TickToMs"))
            value = SongController.TickToMs((int)input);
        if (GUILayout.Button("TickToPos"))
            value = SongController.TickToPos((int)input);

        EditorGUILayout.Separator();
        GUILayout.Label("Seconds ->");

        if (GUILayout.Button("SecToTick"))
            value = SongController.SecToTick(input);
        if (GUILayout.Button("SecToMs"))
            value = SongController.SecToMs(input);
        if (GUILayout.Button("SecToPos"))
            value = SongController.SecToPos(input);

        EditorGUILayout.Separator();
        GUILayout.Label("Milliseconds ->");

        if (GUILayout.Button("MsToTick"))
            value = SongController.MsToTick(input);
        if (GUILayout.Button("MsToSec"))
            value = SongController.MsToSec(input);
        if (GUILayout.Button("MsToPos"))
            value = SongController.MsToPos(input);

        EditorGUILayout.Separator();
        GUILayout.Label("Position (meters) ->");

        if (GUILayout.Button("PosToTick"))
            value = SongController.PosToTick(input);
        if (GUILayout.Button("PosToSec"))
            value = SongController.PosToSec(input);
        if (GUILayout.Button("PosToMs"))
            value = SongController.PosToMs(input);
    }
}
