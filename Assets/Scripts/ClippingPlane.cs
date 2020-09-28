using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ClippingPlane : MonoBehaviour
{
    public MeshFilter filter;

    //material we pass the values to
    Material[] mat;

    void Awake()
    {
        mat = gameObject.GetComponent<MeshRenderer>().materials;
    }

    //execute every frame
    void Update()
    {
        Vector3 normal = new Vector3();
        if (filter && filter.sharedMesh.normals.Length > 0) 
            normal = filter.transform.TransformDirection(filter.sharedMesh.normals[0]);

        Plane plane = new Plane(normal, filter.gameObject.transform.position);

        //transfer values from plane to vector4
        Vector4 planeRepresentation = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);
        //pass vector to shader
        foreach (Material _mat in mat)
            _mat.SetVector("_Plane", planeRepresentation);
    }
}