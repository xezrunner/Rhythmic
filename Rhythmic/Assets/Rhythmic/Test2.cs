using UnityEngine;
using PathCreation;
using static Logger;
using System.Collections.Generic;

public class Test2 : MonoBehaviour {
    VertexPath path;
    PathCreator pathcreator;

    public MeshRenderer meshrenderer;
    public MeshFilter meshfilter;

    void Fail(string message = "Fail.") {
        LogE(message.TM(this));
        Destroy(gameObject);
        return;
    }

    public int obj_count = 6;
    public float obj_width = Variables.TRACK_Width;
    public float obj_length = 60;
    List<PathTransform> objects = new List<PathTransform>();

    void Start() {
        pathcreator = PathTransform.PATH_FindPathCreator();
        path = pathcreator.path;
        if (path == null) Fail("No path.");

        for (int i = 0; i < obj_count; ++i) {
            GameObject obj = new GameObject(i.ToString());
            obj.AddComponent<MeshFilter>().mesh = meshfilter.mesh;
            obj.AddComponent<MeshRenderer>().material = meshrenderer.material;
            PathTransform com = obj.AddComponent<PathTransform>();
            com.pos = new Vector3(((-obj_count * obj_width) / 2f) + (obj_width / 2) + (obj_width * i)
                ,0,0);
            com.desired_size = new Vector3(obj_width, com.desired_size.y, obj_length);
            obj.transform.parent = transform;

            objects.Add(com);
        }
    }

    float prev_obj_width = 0;
    float prev_obj_length = 0;

    float prev_dist = 0;
    public float dist = 0;

    void Update() {
        if (prev_dist != dist || prev_obj_width != obj_width || prev_obj_length != obj_length) {
            prev_dist = dist;
            prev_dist = dist;
            prev_obj_width = obj_width;
            prev_obj_length = obj_length;

            foreach (var o in objects) {
                o.desired_size = new Vector3(obj_width, -1, obj_length);
                o.pos.z = dist;
                o.Deform();
            }
        }
    }
}