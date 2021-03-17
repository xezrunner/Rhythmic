using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FXProperties))]
class FXPropertiesEditor : Editor
{
    FXProperties main;
    TracksController TracksController { get { return TracksController.Instance; } }
    void Awake() => main = (FXProperties)target;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Separator();

        if (!Application.isPlaying)
        {
            EditorGUILayout.LabelField("Tools unavailable until game is stopped.", EditorStyles.boldLabel);
            return;
        }

        // WARNING: these are kinda hacky!

        if (GUILayout.Button("Update destruct FX colors"))
        {
            foreach (AmpTrack t in TracksController.Tracks)
                t.DestructFX.SetupColors();
        }

        // This doesn't quite work right, as there end up being multiple destruct components fighting.
        if (GUILayout.Button("Start destruct FX on current track"))
        {
            main.Destruct_ForceEffects = true;

            foreach (AmpTrack t in TracksController.Tracks)
                t.DestructFX.SetupColors();

            foreach (AmpTrackSection m in TracksController.CurrentTrack.Measures)
                if (m) m.CaptureState = MeasureCaptureState.None;

            TracksController.CurrentTrack.CaptureMeasureAmount(Clock.Instance.Fbar, 3);
        }
    }
}