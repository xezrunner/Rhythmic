using PathCreation;
using System.Collections;
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
        inst_st = Stopwatch.StartNew();
    }

    public float inst_delay_ms = 1000f;

    Stopwatch inst_st;
    bool is_inst_test = true;
    public float inst_time_ms = 0f;
    int inst_i = -1;
    float inst_d = 0;
    int inst_count = 300 * 6;
    void UPDATE_InstTest()
    {
        if (!is_inst_test) return;
        if (inst_delay_ms <= 0) goto instantiation;

        if (inst_time_ms < inst_delay_ms)
        {
            inst_time_ms += Time.unscaledDeltaTime * 1000f;
            return;
        }

        inst_time_ms = 0f;

    instantiation:
        if (inst_i > inst_count)
        {
            is_inst_test = false;
            inst_st.Stop();
            Log("created: %  time: %ms".TM(this), inst_count, inst_st.ElapsedMilliseconds);
            ExampleDebugCom.Instance.Add("time: %ms".Parse(inst_st.ElapsedMilliseconds).TM(this));

            return;
        }
        // Instantiation:
        {
            GameObject obj = Instantiate(Track_Prefab);

            PathTransform p_trans = obj.GetComponent<PathTransform>();
            p_trans.pos = new Vector3(10.8f - (3.6f * (inst_i % 6)), 0, inst_d);
            if (inst_i % 6 == 0) inst_d += 30;

            Track t = obj.GetComponent<Track>();
            t.id_weak = t.id_real = inst_i;

            ++inst_i;
        }

    }

    void Update()
    {
        UPDATE_InstTest();

        if (Keyboard.current.rKey.wasPressedThisFrame)
            SceneManager.LoadScene("test0");
    }
}
