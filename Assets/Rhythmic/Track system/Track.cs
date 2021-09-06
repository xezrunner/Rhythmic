using UnityEngine;

public enum Instrument { Drums, Bass, Synth, Guitar, Vocals, UNKNOWN = -1 }

public class Track : MonoBehaviour
{
    public PathTransform path_trans;
    public MeshFilter mesh_filter;
    public MeshRenderer mesh_renderer;

    public int id_weak; // Identical ID in case of cloned tracks.
    public int id_real; // Unique ID, even for cloned tracks.

    public Instrument instr;

    public void Start()
    {
        // TODO: materials, setup other stuff ...
    }
}
