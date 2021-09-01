using PathCreation;
using System.Diagnostics;
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
        Vector3 v = path.XZ_GetPointAtDistance(0f);
        Vector3 v1 = path.XZ_GetPointAtDistance(-10f);

        Stopwatch st = Stopwatch.StartNew();

        GameObject m = (GameObject)Resources.Load("Models/track_bottom");

        float d = 0;

        for (int i = 0; i < (path.length / 30)*6; ++i)
        {
            GameObject obj = Instantiate(m);
            PathTransform t = obj.AddComponent<PathTransform>();
            t.pos = new Vector3(10.8f - (3.6f * (i % 6)), 0, d);

            if (i % 6 == 0) d += 30;
        }

        //for (int i = 0; i < (path.length / 30); ++i)
        //{
        //    GameObject obj = Instantiate(m);
        //    PathTransform t = obj.AddComponent<PathTransform>();
        //    t.pos = new Vector3(0, 0, d);
        //
        //    d += 30;
        //}

        st.Stop();
        Log("Generated % objects - time: %ms", (int)(path.length / 30) * 6, st.ElapsedMilliseconds);
    }

    float dist_smooth;
    public float smooth = 1.0f;

    Vector3 pos_ref;
    Quaternion rot_ref;
    void Update()
    {
        //cam.transform.localPosition = Vector3.SmoothDamp(cam.transform.localPosition, path.XZ_GetPointAtDistance(dist, offset, offset.x), ref pos_ref, smooth);
        //cam.transform.localRotation = QuaternionUtil.SmoothDamp(cam.transform.localRotation, path.XZ_GetRotationAtDistance(dist, offset.x) * Quaternion.Euler(rot_offset), ref rot_ref, smooth);

        cam.transform.localPosition = path.XZ_GetPointAtDistance(dist, offset, offset.x);
        cam.transform.localRotation = QuaternionUtil.SmoothDamp(cam.transform.localRotation, path.XZ_GetRotationAtDistance(dist, offset.x) * Quaternion.Euler(rot_offset), ref rot_ref, smooth);


        if (!auto) return;

        float delta = Time.deltaTime; // The time elapsed since the last frame.
        float skew = dist - dist_smooth - delta; // The difference in time between the song and the current Clock time
        float smooth_delta = delta + 0.1f * skew; // Smoothen the difference with + (factor * skew)
        dist_smooth += delta + 0.1f * skew; // The smoothened seconds equal fixedDeltaTime + (factor * skew)

        dist += speed * Time.deltaTime;
    }
}