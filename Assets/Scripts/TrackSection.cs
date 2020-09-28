using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackSection : MonoBehaviour
{
    public MeshFilter PlaneMeshFilter;
    public Plane Plane;



    void Awake()
    {
        /*
        // Get clipping plane
        Vector3 normal = new Vector3();
        if (PlaneMeshFilter && PlaneMeshFilter.sharedMesh.normals.Length > 0)
            normal = PlaneMeshFilter.transform.TransformDirection(PlaneMeshFilter.sharedMesh.normals[0]);
        else { Debug.LogError("TrackSection: Clipping plane is not set in prefab or invalid plane mesh!"); return; }

        Plane = new Plane(normal, PlaneMeshFilter.gameObject.transform.position);

        //transfer values from plane to vector4
        Vector4 planeRepresentation = new Vector4(Plane.normal.x, Plane.normal.y, Plane.normal.z, Plane.distance);
        //pass vector to shader
        foreach (Material _mat in mat)
            _mat.SetVector("_Plane", planeRepresentation);*/
    }
}
