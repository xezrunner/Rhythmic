using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Measure))]
public class AmpTrackSectionEditor : Editor
{
    Measure script;
    void Awake() => script = (Measure)target;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Update mesh deformation")) script.DeformMesh();
    }
}