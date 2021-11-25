using UnityEngine;

public class Test2 : MonoBehaviour {
    TrackSystem track_system;

    public Material material;

    public GameObject plane;
    public MeshFilter plane_meshfilter;

    public GameObject inv_plane;
    public MeshFilter inv_plane_meshfilter;

    void Start() {
        track_system = SongSystem.Instance.track_system;
        material = track_system.tracks[0].material;
    }

    bool set_to_enabled = false;
    void Update() {
        if (!material || !plane || !plane_meshfilter || !inv_plane || !inv_plane_meshfilter) return;
        if (!plane_meshfilter) plane_meshfilter = plane.GetComponent<MeshFilter>();

        {
            Vector3 normal = plane_meshfilter.transform.TransformDirection(plane_meshfilter.mesh.normals[0]);
            Plane _plane = new Plane(normal, plane.transform.position);
            Vector4 result = new Vector4(_plane.normal.x, _plane.normal.y, _plane.normal.z, _plane.distance);
            material.SetVector("_Plane", result);
        }

        {
            Vector3 normal = inv_plane_meshfilter.transform.TransformDirection(inv_plane_meshfilter.mesh.normals[0]);
            Plane _plane = new Plane(normal, inv_plane.transform.position);
            Vector4 result = new Vector4(_plane.normal.x, _plane.normal.y, _plane.normal.z, _plane.distance);
            material.SetVector("_InversePlane", result);
        }

        if (!set_to_enabled) {
            material.SetInt("_PlaneEnabled", 1);
            material.SetInt("_InversePlaneEnabled", 1);
            set_to_enabled = true;
        }
    }
}
