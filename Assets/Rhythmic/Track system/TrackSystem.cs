using PathCreation;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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

    [Header("Prefabs")]
    public GameObject Track_Prefab;

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

        float d = 0;
        //int count = (int)(path.length / 30) * 6;
        int count = 300 * 6;

        for (int i = 0; i < count; ++i)
        {
            GameObject obj = Instantiate(Track_Prefab);

            PathTransform p_trans = obj.GetComponent<PathTransform>();
            p_trans.pos = new Vector3(10.8f - (3.6f * (i % 6)), 0, d);
            if (i % 6 == 0) d += 30;

            Track t = obj.GetComponent<Track>();
            t.id_weak = t.id_real = i;
        }

        st.Stop();
        Log("time: %ms".TM(this), st.ElapsedMilliseconds);
        ExampleDebugCom.Instance.Add("time: %ms".Parse(st.ElapsedMilliseconds).TM(this));
    }

    void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
            SceneManager.LoadScene("test0");
    }
}
