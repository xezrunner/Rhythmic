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

        if (GUILayout.Button("OG"))
        {
            script.mesh = script.ogMesh;
            script.meshFilter.mesh = script.ogMesh;
            script.mesh = script.ogMesh = null;
            if (script.ogPos.HasValue)
            { script.targetObject.transform.position = script.ogPos.Value; script.ogPos = null; }
            script.ClearAllCubes();
        }

        if (GUILayout.Button("Reset"))
        {
            script.targetObject = null;
            script.mesh = script.ogMesh = null;
            if (script.ogPos.HasValue)
                script.targetObject.transform.position = script.ogPos.Value;
            script.ogPos = null;
            script.ClearAllCubes();
        }

        if (GUILayout.Button("Debug at Z"))
            script.Debug_CreateObjectAtZ();

        //if (script.targetObject != null & script.ogMesh != null)
        //{
        //    script.mesh = script.ogMesh;
        //    script.meshFilter.mesh = script.ogMesh;
        //    script.mesh = script.ogMesh = null;

        //    script.DeformMesh();
        //}

    }

}