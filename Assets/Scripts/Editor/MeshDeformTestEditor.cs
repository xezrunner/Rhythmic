using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshDeformTest))]
public class MeshDeformTestEditor : Editor
{
    public void Awake() => script = (MeshDeformTest)target; 
    MeshDeformTest script;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Deform mesh"))
            script.DeformMesh();
    }

}