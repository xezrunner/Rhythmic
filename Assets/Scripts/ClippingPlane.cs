#undef LIVE_UPDATE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClippingPlane : MonoBehaviour
{
    public MeshRenderer MeshRenderer;
    public MeshFilter filter;
    public MeshFilter inverse_filter;

    // Materials to pass values to
    Material[] mat;

    void Awake()
    {
        if (MeshRenderer != null)
            GetMaterials();
    }
    void Start() => Clip();

#if LIVE_UPDATE // Live test clipping for dev testing purposes only!
    void Update() => Clip();
#endif

    void GetMaterials()
    {
        if (MeshRenderer)
            mat = MeshRenderer.materials;
        else
            mat = gameObject.GetComponent<MeshRenderer>().materials;
    }

    void Clip()
    {
        if (mat == null) GetMaterials();

        Vector4 planeRepresentation = Vector4.zero;
        Vector4 inverse_planeRepresentation = Vector4.zero;

        // Regular plane
        if (filter)
        {
            Vector3 normal = new Vector3();
            if (filter && filter.sharedMesh.normals.Length > 0)
                normal = filter.transform.TransformDirection(filter.sharedMesh.normals[0]);

            Plane plane = new Plane(normal, filter.gameObject.transform.position);

            //transfer values from plane to vector4
            planeRepresentation = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);
        }

        // Inverse plane
        if (inverse_filter)
        {
            Vector3 inverse_normal = new Vector3();
            if (inverse_filter && inverse_filter.sharedMesh.normals.Length > 0)
                inverse_normal = inverse_filter.transform.TransformDirection(inverse_filter.sharedMesh.normals[0]);

            Plane inverse_plane = new Plane(inverse_normal, inverse_filter.gameObject.transform.position);

            //transfer values from plane to vector4
            inverse_planeRepresentation = new Vector4(inverse_plane.normal.x, inverse_plane.normal.y, inverse_plane.normal.z, inverse_plane.distance);
        }

        //pass vector to shader

        /*
        if (MaterialID != -1)
        {
            mat = gameObject.GetComponent<MeshRenderer>().materials;
            mat[MaterialID].SetVector("_Plane", planeRepresentation);
        }
        else*/

        foreach (Material _mat in mat)
        {
            if (filter)
            {
                _mat.SetVector("_Plane", planeRepresentation);
                _mat.SetInt("_PlaneEnabled", 1);
            }
            else
                _mat.SetInt("_PlaneEnabled", 0);

            if (inverse_filter)
            {
                _mat.SetVector("_InversePlane", inverse_planeRepresentation);
                _mat.SetInt("_InversePlaneEnabled", 1);
            }
            else
                _mat.SetInt("_InversePlaneEnabled", 0);
        }
    }
}