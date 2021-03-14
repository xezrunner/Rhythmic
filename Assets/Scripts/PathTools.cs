using UnityEngine;
using PathCreation;

public static class PathTools
{
    public static VertexPath Path; // Global Path
    public static WorldSystem WorldSystem { get { return WorldSystem.Instance; } }

    // TODO: add overloads without 'path'
    public static Vector3 GetPositionOnPath(VertexPath path, float distance) => GetPositionOnPath(path, distance, Vector3.zero);
    public static Vector3 GetPositionOnPath(VertexPath path, float distance, Vector3 offset)
    {
        Vector3 tunnelCenter = (Tunnel.Instance) ? Tunnel.Instance.center : Vector3.zero;

        float dist = distance; // distance on path, 0 or path.length if out of path
        bool outOfPath = false; // controls whether offsetting is required for exceeding the path

        if (path == null)
        {
            return new Vector3(0, 0, dist) +
                          Vector3.right * tunnelCenter.x + // Translate to offset
                          Vector3.up * tunnelCenter.y +
                          Vector3.right * offset.x +
                          Vector3.up * offset.y;
        }

        if (distance < 0) { dist = 0; outOfPath = true; }
        else if (distance > path.length)
        {
            dist = path.length - 0.01f; // HACK!!!
            distance = distance - path.length; // distance for offsetting needs to be the difference between path.length and desired distance
            outOfPath = true;
        }

        // Normals
        /* Previous code - this has been optimized with potentially the exact same results.
        Vector3 normal = path.GetNormalAtDistance(dist);
        Vector3 localUp = Vector3.Cross(path.GetTangentAtDistance(dist), normal);
        Vector3 localRight = normal;
        */

        Vector3 localRight = path.GetNormalAtDistance(dist);
        Vector3 localUp = Quaternion.Euler(0, 0, 90) * localRight;

        // The point on the path
        // In case of a negative distance, it's the very beginning of the path.
        Vector3 pointOnPath = path.GetPointAtDistance(dist);

        // Handle negative/exceeded position
        if (outOfPath)
        {
            Quaternion rot = GetRotationOnPath(path, dist); // Get rotation for the very beginning or end of the path
            pointOnPath += rot * new Vector3(0, 0, distance); // Traverse backwards from the path starting point direction
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
        if (path == null)
            return Quaternion.identity * Quaternion.Euler(offset);

        if (distance < 0f) distance = 0; // In case of negative position, zero out the distance
        else if (distance > path.length) distance = path.length - 0.01f; // HACK!!!

        return path.GetRotationAtDistance(distance) * Quaternion.Euler(0, 0, 90) * Quaternion.Euler(offset);
    }

    // Key: [prev, next dist] | Value: values
    public static Vector3[] GetFunkyContour(float distance)
    {
        if (!WorldSystem)
        { Logger.LogMethodE("No WorldSystem!"); return null; }

        // Find closest index to distance:
        Vector3[] funkyContour = WorldSystem.FunkyContour;

        if (funkyContour.Length < 2)
        { Logger.LogMethodE("Funky contour count has to be greater than 2!"); return null; }

        for (int i = 0; i < funkyContour.Length; i++)
        {
            Vector3 v_main = funkyContour[i];
            Vector3 v_prev = i > 0 ? funkyContour[i - 1] : new Vector3(-1, -1, -1);
            //Vector3 v_next = i < funkyContour.Length - 1 ? funkyContour[i + 1] : new Vector3(-1, -1);

            if (i == 0 && v_main.x < distance) continue;
            else if (i == funkyContour.Length - 1) return new Vector3[2] { v_main, v_main }; // TODO: return ([-1,-1], v_main) instead?

            if (v_main.x >= distance && v_prev.x != -1)
                return new Vector3[2] { v_prev, v_main };
        }

        Logger.LogMethodE("Failed!");
        return null;
    }
}