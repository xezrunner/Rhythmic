using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AmpTrackSection))]
public class AmpTrackSectionEditor : Editor
{
    AmpTrackSection script;
    void Awake() => script = (AmpTrackSection)target;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Update mesh deformation")) script.DeformMesh();
    }
}