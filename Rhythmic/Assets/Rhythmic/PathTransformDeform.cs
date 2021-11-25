using UnityEngine;

public partial class PathTransform {
    // int deform_counter = -1;
    public void Deform() {
        if (Application.isEditor && !Application.isPlaying) return;
        if (vertex_count < 1) return;

        // Log("Deform called by % - %", gameObject.name, ++deform_counter);

        for (int i = 0; i < vertex_count; ++i) {
            Vector3 pos_xy = new Vector2(pos.x, pos.y); // Position X and Y

            Vector3 v = OG_vertices[i]; // Vertex
            Vector3 v_xy = new Vector2(v.x, v.y); // Vertex X and Y

            float dist = pos.z;

            // Move origin point:
            if (origin_mode == OriginMode.Front) v.z += max_values.z;
            else if (origin_mode == OriginMode.Back) {
                v.z -= max_values.z;
                dist += max_values_double.z;
            } else if (origin_mode == OriginMode.Custom) v.z += origin_custom;

            // TODO: The clamping of these could be done in UpdateClipValues().
            float min_clip_clamped = Mathf.Clamp(min_clip_frac, 0f, max_clip_frac);
            float max_clip_clamped = Mathf.Clamp(max_clip_frac, min_clip_clamped, 1f);
            float z_frac = Mathf.Clamp(v.z / max_values_double.z, min_clip_clamped, max_clip_clamped);

            float v_z = Mathf.Lerp(0, desired_size.z, z_frac);
            dist += v_z;

            float x_rot = pos_xy.x + (v.x + max_values.x);

            vertices[i] = path.XZ_GetPointAtDistance(dst: dist, pos: pos_xy + v_xy, p_x_rot: x_rot);
        }

        mesh.SetVertices(vertices);
        mesh.RecalculateBounds();
    }
}