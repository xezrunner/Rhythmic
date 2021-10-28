using UnityEngine;
using static Logger;

public class Measure : MonoBehaviour
{
    public static GameObject measure_prefab;

    public static Measure CreateMeasure(Track parent_track, int id)
    {
        if (!measure_prefab) measure_prefab = (GameObject)Resources.Load("Prefabs/Measure");
        GameObject go = Instantiate(measure_prefab, parent_track.transform);
        go.name = "%::%".Parse(parent_track.track_name, id);

        Measure m = go.GetComponent<Measure>();
        m.id = id;
        m.parent_track = parent_track;
        parent_track.measures[id] = m;

        Vector3 pos = new Vector3(3.68f * parent_track.id_real, 0, 32.89f * id);
        m.trans0.pos = pos;

        return m;
    }

    public PathTransform trans0;

    public Track parent_track;
    public int id;

    public float distance;

    void Awake()
    {
        
    }

    void Start()
    {
        //mesh_renderer.material.SetColor("_EmissionColor", RHX_Colors.track_colors[(int)parent_track.instrument]);
    }
}

public static class RHX_Colors
{
    public static Color[] track_colors = new Color[]
    {
        ConvertFrom255(new Color(255, 61, 246, 255)),
        ConvertFrom255(new Color(9, 79, 255, 255)),
        ConvertFrom255(new Color(218, 195, 43, 255)),
        ConvertFrom255(new Color(213, 21, 11, 255)),
        ConvertFrom255(new Color(32, 202, 45, 255)),
        ConvertFrom255(new Color(110, 110, 110, 255))
    };

    public static Color Drums = ConvertFrom255(new Color(255, 61, 246, 255));
    public static Color Bass = ConvertFrom255(new Color(9, 79, 255, 255));
    public static Color Synth = ConvertFrom255(new Color(218, 195, 43, 255));
    public static Color Guitar = ConvertFrom255(new Color(213, 21, 11, 255));
    public static Color Vocals = ConvertFrom255(new Color(32, 202, 45, 255));
    public static Color Freestyle = ConvertFrom255(new Color(110, 110, 110, 255));

    public static Color ConvertFrom255(Color c) => new Color(c.r / 255, c.g / 255, c.b / 255, c.a / 255);
}