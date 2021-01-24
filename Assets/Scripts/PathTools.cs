using UnityEngine;
using PathCreation;

public static class PathTools
{
    public static VertexPath Path; // Global Path

    public static Vector3 GetPositionOnPath(VertexPath path, float distance) => GetPositionOnPath(path, distance, Vector3.zero);
    public static Vector3 GetPositionOnPath(VertexPath path, float distance, Vector3 offset)
    {
        Vector3 tunnelCenter = (Tunnel.Instance) ? Tunnel.Instance.center : Vector3.zero;

        float dist = distance; // distance on path, 0 or path.length if out of path
        bool outOfPath = false; // controls whether offsetting is required for exceeding the path

        if (distance < 0) { dist = 0; outOfPath = true; }
        else if (distance > path.length)
        {
            dist = path.length - 0.01f; // HACK!!!
            distance = distance - path.length; // distance for offsetting needs to be the difference between path.length and desired distance
            outOfPath = true;
        }

        // Normals
        Vector3 localUp = Vector3.Cross(path.GetTangentAtDistance(dist), path.GetNormalAtDistance(dist));
        Vector3 localRight = path.GetNormalAtDistance(dist);

        // The point on the path
        // In case of a negative distance, it's the very beginning of the path.
        Vector3 pointOnPath = path.GetPointAtDistance(dist);

        // Handle negative/exceeded position
        if (outOfPath)
        {
            Quaternion rot = GetRotationOnPath(path, dist); // Get rotation for the very beginning or end of the path
            pointOnPath += (rot * new Vector3(0, 0, distance)); // Traverse backwards from the path starting point direction
        }

        Vector3 finalVec = pointOnPath +
                      localRight * tunnelCenter.x + // Translate to offset
                      localUp * tunnelCenter.y +
                      localRight * offset.x +
                      localUp * offset.y;

        return finalVec;
    }

    public static Quaternion GetRotationOnPath(VertexPath path, float distance) => GetRotationOnPath(path, distance, Vector3.zero);
    public static Quaternion GetRotationOnPath(VertexPath path, float distance, Vector3 offset)
    {
        if (distance < 0f) distance = 0; // In case of negative position, zero out the distance
        else if (distance > path.length) distance = path.length - 0.01f; // HACK!!!

        return path.GetRotationAtDistance(distance) * Quaternion.Euler(0, 0, 90) * Quaternion.Euler(offset);
    }
}