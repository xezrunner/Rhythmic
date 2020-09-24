using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;

[CustomEditor(typeof(TrackMeshCreator))]
public class TrackSectionCreatorEditor : PathSceneToolEditor
{
    int numberOfSections;
    TrackMeshCreator script;

    void Awake() => script = (TrackMeshCreator)target;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Label(string.Format("Curavture distance in zPos: {0}", script.pathCreator.path.length));
        GUILayout.Label(string.Format("Track objects: {0}", script.TrackObjects.Count));

        if (GUILayout.Button("Create object"))
            script.Debug_CreateTestMesh(Keyboard.current.leftShiftKey.isPressed, Keyboard.current.leftCtrlKey.isPressed);
        if (GUILayout.Button("Delete all test objects"))
        {
            script.TrackObjects.ForEach(g => DestroyImmediate(g));
            script.TrackObjects.Clear();
            script.debug_xPosition = 0;
            script.debug_startPoint = 0f;
        }
        if (GUILayout.Button("Test create sections for full curvature length"))
        {
            script.debug_startPoint = 0f;

            int totalNumberOfSections = (int)(script.pathCreator.path.length / script.debug_length);
            for (int i = 0; i < totalNumberOfSections; i++)
                script.Debug_CreateTestMesh(true, true);
        }

        numberOfSections = EditorGUILayout.IntSlider(numberOfSections, 1, 100);

        if (GUILayout.Button("Test create sections for given value above"))
        {
            float ogStartPoint = script.debug_startPoint;

            for (int i = 0; i < numberOfSections; i++)
                script.Debug_CreateTestMesh(true, true);

            script.debug_startPoint = ogStartPoint;

            if (Keyboard.current.leftShiftKey.isPressed)
                script.debug_xPosition++;
        }
    }
}
