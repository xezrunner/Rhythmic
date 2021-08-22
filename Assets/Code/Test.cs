using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Logger;

public class Test : MonoBehaviour
{
    MeshDeformer mesh_deformer;
    void Awake() => mesh_deformer = MeshDeformer.Instance;

    
}

[CustomEditor(typeof(Test))]
public class MeshDeformerEditor : Editor
{
    Test main;
    void Awake() => main = (Test)target;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}