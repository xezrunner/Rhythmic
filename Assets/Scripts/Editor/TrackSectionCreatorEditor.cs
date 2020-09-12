using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrackSectionCreator))]
public class TrackSectionCreatorEditor : Editor
{


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TrackSectionCreator script = (TrackSectionCreator)target;

        if (GUILayout.Button("Create object"))
            script.CreateTrackMesh();
    }
}
