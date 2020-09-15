using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrackMeshCreator))]
public class TrackSectionCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TrackMeshCreator script = (TrackMeshCreator)target;

        if (GUILayout.Button("Create object"))
            script.CreateTrackMesh();
    }
}
