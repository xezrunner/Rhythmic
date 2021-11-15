using UnityEngine;
using static Logger;

public partial class PathTransform
{
    // int deform_counter = -1;
    public void Deform()
    {
        if (Application.isEditor && !Application.isPlaying) return;
        if (vertex_count < 1) return;

        // Log("Deform called by % - %", gameObject.name, ++deform_counter);

        for (int i = 0; i < vertex_count; ++i)
        {
            Vector3 pos_xy = new Vector2(pos.x, pos.y); // Position X and Y

            Vector3 v = OG_vertices[i]; // Vertex
            Vector3 v_xy = new Vector2(v.x, v.y); // Vertex X and Y

            // TODO: Figure out what's wrong here: 
            float v_z = Mathf.LerpUnclamped(0, desired_size.z / 2, v.z / max_values.z);
            if (push_origin_to_front) v_z += max_values.z;
            float dist = (pos.z + v_z);
            // Move origin point:
            if (origin_mode == OriginMode.Front) v.z += max_values.z;
            else if (origin_mode == OriginMode.Custom) v.z += origin_custom;
            else if (origin_mode == OriginMode.Back)
            {
                v.z -= max_values.z;
                dist += max_values_double.z;
            }

            float x_rot = pos_xy.x + (v.x + max_values.x);

            vertices[i] = path.XZ_GetPointAtDistance(dst: dist, pos: pos_xy + v_xy, x_rot: x_rot);
        }

        mesh.SetVertices(vertices);
        mesh.RecalculateBounds();
    }
}