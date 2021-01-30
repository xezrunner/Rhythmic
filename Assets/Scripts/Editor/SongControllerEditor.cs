using UnityEngine;
using UnityEditor;

// TODO: We are working with AmplitudeSongController for the time being. This will need to change to SongController later!
[CustomEditor(typeof(AmplitudeSongController))]
public class SongControllerEditor : Editor
{
    AmplitudeSongController SongController;
    void Awake() => SongController = (AmplitudeSongController)target;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        DrawTimeUnits();
        DrawMisc();
    }

    bool timeunits_foldOut = false;
    void DrawTimeUnits()
    {
        timeunits_foldOut = EditorGUILayout.BeginFoldoutHeaderGroup(timeunits_foldOut, "Time units debug");

        if (!timeunits_foldOut) { EditorGUILayout.EndFoldoutHeaderGroup(); return; }

        SongController.beatTicks = EditorGUILayout.IntField("Beat ticks: ", SongController.beatTicks);
        SongController.songFudgeFactor = EditorGUILayout.Slider("Tunnel scaling: ", SongController.songFudgeFactor, overrideTunnelScaleLimit ? -4 : 0.5f, overrideTunnelScaleLimit ? 4 : 1.5f);

        GUILayout.Space(10);

        string timeunits = $"measureLengthInzPos: {SongController.measureLengthInzPos}\n" +
                           $"subbeatLengthInzPos: {SongController.subbeatLengthInzPos}\n" +
                           "\n" +
                           $"beatPerSec: {SongController.beatPerSec}\n" +
                           $"secPerBeat: {SongController.secPerBeat}\n" +
                           "\n" +
                           $"" +
                           $"tickInSec: {SongController.tickInSec}\n" +
                           $"tickInMs: {SongController.tickInMs}\n" +
                           $"tickInPos: {SongController.tickInPos}\n" +
                           "\n" +
                           $"secInTick: {SongController.secInTick}\n" +
                           $"secInMs: {SongController.secInMs}\n" +
                           $"secInPos: {SongController.secInPos}\n" +
                           "\n" +
                           $"posInTick: {SongController.posInTick}\n" +
                           $"posInSec: {SongController.posInSec}\n" +
                           $"posInMs: {SongController.posInMs}\n";
        GUILayout.Label(timeunits);

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    bool misc_foldOut = false;
    bool overrideTunnelScaleLimit = false;
    void DrawMisc()
    {
        misc_foldOut = EditorGUILayout.BeginFoldoutHeaderGroup(misc_foldOut, "Miscellaneous settings");
        if (!misc_foldOut) { EditorGUILayout.EndFoldoutHeaderGroup(); return; }

        overrideTunnelScaleLimit = GUILayout.Toggle(overrideTunnelScaleLimit, " Override tunnel scale limit");

        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}