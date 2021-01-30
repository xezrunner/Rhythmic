#undef LIVE_UPDATE

using System.Collections.Generic;
using UnityEngine;

public class ClipManager : MonoBehaviour
{
    public bool GetMaterialsOnStart = false;
    public bool ClipOnStart = false;
    public bool DisableClippingOnNull = false;

    public List<MeshRenderer> MeshRenderers = new List<MeshRenderer>();
    public GameObject plane;
    public GameObject inverse_plane;

    // Materials to pass values to
    public List<Material> materials = new List<Material>();

    void Awake() => GetMaterials();
    void Start()
    {
        if (ClipOnStart) Clip();
    }

#if LIVE_UPDATE // Live test clipping for dev testing purposes only!
    void Update() => Clip();
#endif

    public void AddMeshRenderer(MeshRenderer mr) { foreach (Material m in mr.materials) materials.Add(m); }
    public void AddMaterial(Material m) => materials.Add(m);

    void GetMaterials()
    {
        if (MeshRenderers.Count > 0)
            foreach (MeshRenderer r in MeshRenderers)
                foreach (Material m in r.materials)
                    materials.Add(m);
        else
        {
            MeshRenderer r = gameObject.GetComponent<MeshRenderer>();
            if (!r) return;

            foreach (Material m in r.materials)
                materials.Add(r.material);
        }
    }

    public void Clip()
    {
        if (materials == null) GetMaterials();

        Vector4 planeRepresentation = Vector4.zero;
        Vector4 inverse_planeRepresentation = Vector4.zero;

        // Regular plane
        if (plane)
        {
            MeshFilter plane_mesh = plane.GetComponent<MeshFilter>();

            Vector3 normal = new Vector3();
            if (plane_mesh && plane_mesh.mesh.normals.Length > 0)
                normal = plane_mesh.transform.TransformDirection(plane_mesh.mesh.normals[0]);

            Plane _plane = new Plane(normal, plane.transform.position);

            //transfer values from plane to vector4
            planeRepresentation = new Vector4(_plane.normal.x, _plane.normal.y, _plane.normal.z, _plane.distance);
        }

        // Inverse plane
        if (inverse_plane)
        {
            MeshFilter plane_mesh = inverse_plane.GetComponent<MeshFilter>();

            Vector3 normal = new Vector3();
            if (plane_mesh && plane_mesh.mesh.normals.Length > 0)
                normal = plane_mesh.transform.TransformDirection(plane_mesh.mesh.normals[0]);

            Plane _plane = new Plane(normal, inverse_plane.transform.position);

            //transfer values from plane to vector4
            inverse_planeRepresentation = new Vector4(_plane.normal.x, _plane.normal.y, _plane.normal.z, _plane.distance);
        }

        //pass vector to shader

        foreach (Material _mat in materials)
        {
            if (plane)
            {
                _mat.SetVector("_Plane", planeRepresentation);
                _mat.SetFloat("_PlaneEnabled", 1);
            }
            else if (DisableClippingOnNull)
                _mat.SetFloat("_PlaneEnabled", 0);

            if (inverse_plane)
            {
                _mat.SetVector("_InversePlane", inverse_planeRepresentation);
                _mat.SetFloat("_InversePlaneEnabled", 1);
            }
            else if (DisableClippingOnNull)
                _mat.SetFloat("_InversePlaneEnabled", 0);
        }
    }
}