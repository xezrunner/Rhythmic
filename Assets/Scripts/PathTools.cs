using UnityEngine;
using PathCreation;

public static class PathTools
{
    public static Vector3 GetPositionOnPath(VertexPath path, float distance) => GetPositionOnPath(path, distance, Vector3.zero);
    public static Vector3 GetPositionOnPath(VertexPath path, float distance, Vector3 offset)
    {
        Vector3 tunnelCenter = (Tunnel.Instance) ? Tunnel.Instance.center : Vector3.zero;

        if (distance > 0f)
        {
            Vector3 localUp = Vector3.Cross(path.GetTangentAtDistance(distance), path.GetNormalAtDistance(distance));
            Vector3 localRight = path.GetNormalAtDistance(distance);

            return path.GetPointAtDistance(distance) +
                        localRight * tunnelCenter.x +
                        localUp * tunnelCenter.y +
                        localRight * offset.x +
                        localUp * offset.y;
        }
        else // NEGATIVE POSITION
        {
            Vector3 localUp = Vector3.Cross(path.GetTangentAtDistance(0), path.GetNormalAtDistance(0));
            Vector3 localRight = path.GetNormalAtDistance(0);

            Quaternion rot = GetRotationOnPath(path, 0);

            Vector3 pathVec = path.GetPointAtDistance(0) + (rot * new Vector3(0, 0, distance));
            Vector3 vec = pathVec +
                          localRight * tunnelCenter.x +
                          localUp * tunnelCenter.y +
                          localRight * offset.x +
                          localUp * offset.y;

            return vec;
        }
    }

    public static Quaternion GetRotationOnPath(VertexPath path, float distance) => GetRotationOnPath(path, distance, Vector3.zero);
    public static Quaternion GetRotationOnPath(VertexPath path, float distance, Vector3 offset)
    {
        if (distance < 0f) distance = 0; // In case of negative position, zero out the distance
        return path.GetRotationAtDistance(distance) * Quaternion.Euler(0, 0, 90) * Quaternion.Euler(offset);
    }
}