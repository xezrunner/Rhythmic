using UnityEngine;
using static Logger;

public class Measure : MonoBehaviour
{
    public static Measure CreateMeasure(Track parent_track, int id)
    {
        GameObject go = new GameObject("%::%".Parse(parent_track.track_name, id));
        Measure m = go.AddComponent<Measure>();
        m.parent_track = parent_track;
        m.id = id;

        go.transform.parent = parent_track.transform;
        return m;
    }

    public PathTransform path_trans;

    public Track parent_track;
    public int id;

    public MeshRenderer mesh_renderer;
    public MeshFilter mesh_filter;

    public float distance;

    void Start()
    {
        //Log("Measure ID % of track % initialized.", id, parent_track.instrument);
    }
}