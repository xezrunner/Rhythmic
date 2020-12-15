using UnityEngine;
using PathCreation;

public static class PathTools
{
    public static Vector3 GetPositionOnPath(VertexPath path, float distance) => GetPositionOnPath(path, distance, Vector3.zero);
    public static Vector3 GetPositionOnPath(VertexPath path, float distance, Vector3 offset)
    {
        if (Tunnel.Instance is null)
        {
            Debug.LogError("PathTools: Tunnel was null!");
            return new Vector3();
        }

        Vector3 localUp = Vector3.Cross(path.GetTangentAtDistance(distance), path.GetNormalAtDistance(distance));
        Vector3 localRight = path.GetNormalAtDistance(distance);

        return path.GetPointAtDistance(distance) +
                    localRight * Tunnel.Instance.center.x +
                    localUp * Tunnel.Instance.center.y +
                    localRight * offset.x +
                    localUp * offset.y;
    }

    public static Quaternion GetRotationOnPath(VertexPath path, float distance) => GetRotationOnPath(path, distance, Vector3.zero);
    public static Quaternion GetRotationOnPath(VertexPath path, float distance, Vector3 offset)
    {
        return path.GetRotationAtDistance(distance) * Quaternion.Euler(0, 0, 90) * Quaternion.Euler(offset);
    }
}