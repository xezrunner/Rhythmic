using PathCreation;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using static Logger;

public class Test : MonoBehaviour
{
    public Camera cam;
    public PathCreator pathcreator;
    VertexPath path { get { return pathcreator.path; } }

    public bool auto = true;
    public float dist;
    public float speed = 1f;
    public Vector3 offset;
    public Vector3 rot_offset;

    void Start()
    {
        Stopwatch st = Stopwatch.StartNew();

        GameObject m = (GameObject)Resources.Load("Models/track_bottom");

        float d = 25;

        for (int i = 0; i < 7*6; ++i)
        {
            GameObject obj = Instantiate(m);
            PathTransform t = obj.AddComponent<PathTransform>();
            t.pos = new Vector3(3.6f * (i % 6), 0, d);

            if (i % 6 == 0) d += 25;
        }

        st.Stop();
        Log("Generated % objects - time: %ms", 7*6, st.ElapsedMilliseconds);
    }

    void Update()
    {
        cam.transform.localPosition = path.XZ_GetPointAtDistance(dist, offset, offset.x);
        cam.transform.localRotation = path.XZ_GetRotationAtDistance(dist, offset.x) * Quaternion.Euler(rot_offset);

        if (auto)
            dist += speed * Time.deltaTime;
    }
}

[CustomEditor(typeof(Test))]
public class MeshDeformerEditor : Editor
{
    Test main;
    void Awake() => main = (Test)target;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}