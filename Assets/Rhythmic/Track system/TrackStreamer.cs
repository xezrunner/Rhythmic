using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using static Logger;

public class TrackStreamer : MonoBehaviour
{
    public static TrackStreamer Instance;
    GameVariables Vars;

    public Transform trans;

    [Header("Prefabs")]
    public GameObject Track_Prefab;

    public void Awake()
    {
        Instance = this;
        Vars = GameState.Variables;
    }

    public void Start()
    {
        //Test();
    }

    #region Testing
    Stopwatch test_timer;
    Stopwatch test_i_timer;
    public List<float> test_i_times;

    bool test_active;

    int test_count = 300 * 6;
    int test_i = -1; float test_d = 0;
    float test_time;
    void Test()
    {
        test_active = true;
        test_timer = Stopwatch.StartNew();
        test_i = -1; test_time = 0f;
        test_i_times = new List<float>();
    }
    void UPDATE_Test()
    {
        if (!test_active) return;

        // TEST OVER: 
        if (test_i > test_count)
        {
            test_active = false;
            test_timer.Stop();

            Log("created: %  time: %ms".TM(this), test_count, test_timer.ElapsedMilliseconds);
            ExampleDebugCom.Instance.Add("created: %  time: %ms".Parse(test_count, test_timer.ElapsedMilliseconds).TM(this));

            string individual = "individual inst: average: %, low: %, max: %".Parse(test_i_times.Average(), test_i_times.Min(), test_i_times.Max());
            Log(individual.TM(this));
            ExampleDebugCom.Instance.Add(individual.TM(this));

            return;
        }

        if (Vars.inst_delay_ms != 0 && test_time < Vars.inst_delay_ms)
        {
            test_time += Time.unscaledDeltaTime;
            return;
        }

        test_time = 0f;
        Test_InstantiateTrack();
    }

    void Test_InstantiateTrack()
    {
        test_i_timer = Stopwatch.StartNew();

        GameObject obj = Instantiate(Track_Prefab);
        obj.transform.parent = trans;

        PathTransform p_trans = obj.GetComponent<PathTransform>();
        p_trans.pos = new Vector3(10.8f - (3.6f * (test_i % 6)), 0, test_d);
        if (test_i % 6 == 0) test_d += 30;

        Track t = obj.GetComponent<Track>();
        t.id_weak = t.id_real = test_i;

        test_i_timer.Stop();
        test_i_times.Add(test_i_timer.ElapsedMilliseconds);

        ++test_i;
    }
    #endregion

    public void Update()
    {
        UPDATE_Test();
    }
}
