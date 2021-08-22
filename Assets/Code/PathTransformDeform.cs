using UnityEngine;
using static Logger;

public partial class PathTransform
{
    int deform_counter = -1;
    public void Deform()
    {
        if (Application.isEditor && !Application.isPlaying) return;

        Log("Deform called by % - %", gameObject.name, ++deform_counter);

        for (int i = 0; i < vertex_count; ++i)
        {
            Vector3 v = OG_vertices[i];
            Vector3 v_xy = new Vector2(v.x, v.y);

            Vector3 pos_xy = new Vector3(pos.x, pos.y);
            float dist = (pos.z + v.z);

            float x_rot = pos_xy.x + (v.x + max_values.x);

            Vector3 point = path.XZ_GetPointAtDistance(dist, pos_xy + v_xy, x_rot);

            Vector3 final = point;
            vertices[i] = final;    
        }

        mesh.SetVertices(vertices);
        mesh.RecalculateBounds();
    }
}