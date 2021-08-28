using PathCreation;
using System.Diagnostics;
using UnityEngine;
using static Logger;

public class TrackSystem : MonoBehaviour
{
    public static TrackSystem Instance;

    public PathCreator pathcreator;
    public WorldSystem worldsystem;

#if UNITY_EDITOR
    public VertexPath path { get { return pathcreator.path; } }
#else
    public VertexPath path;
#endif

    public void Awake()
    {
        Instance = this;
        worldsystem = WorldSystem.Instance;
        pathcreator = WorldSystem.GetAPathCreator();
        if (!pathcreator && LogE("No pathcreator!".T(this))) return;

        // Changes to the path are reflected in debug builds:
#if !UNITY_EDITOR
        path = pathcreator.path;
#endif
    }

    public void Start()
    {
        Stopwatch st = Stopwatch.StartNew();

        GameObject m = (GameObject)Resources.Load("Models/track_bottom");

        float d = 0;
        int count = (int)(path.length / 30) * 6;

        for (int i = 0; i < count; ++i)
        {
            GameObject obj = Instantiate(m);
            PathTransform t = obj.AddComponent<PathTransform>();
            t.pos = new Vector3(10.8f - (3.6f * (i % 6)), 0, d);

            if (i % 6 == 0) d += 30;
        }

        st.Stop();
        Log("Generated % objects - time: %ms", count, st.ElapsedMilliseconds);
    }
}
