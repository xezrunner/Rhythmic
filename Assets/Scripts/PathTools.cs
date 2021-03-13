using UnityEngine;
using PathCreation;

// TODO: Potentially move to TrackGeo.cs?
public struct TrackGeo
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 FunkyContour;

    public TrackGeo(Vector3 pos, Quaternion rot, Vector3 funky)
    { Position = pos; Rotation = rot; FunkyContour = funky; }
}

public static class PathTools
{
    public static VertexPath Path; // Global Path
    static WorldSystem WorldSystem { get { return WorldSystem.Instance; } }
    static TracksController TracksController { get { return TracksController.Instance; } }

    public static TrackGeo GetTrackGeo(float distance, Vector3 position, Vector3 rotation)
    {
        Vector3 tunnelCenter = (Tunnel.Instance) ? Tunnel.Instance.center : Vector3.zero;

        float dist = distance; // distance on path, 0 or path.length if out of path
        bool outOfPath = false; // controls whether offsetting is required for exceeding the path

        if (distance < 0) { dist = 0; outOfPath = true; }
        else if (distance > Path.length)
        {
            dist = Path.length - 0.01f; // HACK!!!
            distance = distance - Path.length; // distance for offsetting needs to be the difference between Path.length and desired distance
            outOfPath = true;
        }

        // Calculate position:
        Vector3 localRight = Path.GetNormalAtDistance(dist);
        Vector3 localUp = Quaternion.Euler(0, 0, 90) * localRight;

        Vector3 pointOnPath = Path.GetPointAtDistance(dist);

        if (outOfPath) // Handle negative/exceeded position
        {
            Quaternion pos_rot = GetRotationOnPath(Path, dist); // Get rotation for the very beginning or end of the path
            pointOnPath += pos_rot * new Vector3(0, 0, distance); // Traverse backwards from the path starting point direction
        }

        Vector3 pos = pointOnPath +
                      localRight * tunnelCenter.x + // Translate to offset
                      localUp * tunnelCenter.y +
                      localRight * position.x +
                      localUp * position.y;

        // Calculate rotation:
        Quaternion rot = Path.GetRotationAtDistance(dist) * Quaternion.Euler(0, 0, 90) * Quaternion.Euler(position); // TODO: Figure out 90 rotation (model)

        // Funky contour
        Vector3 funkyContour = LerpFunkyContour(GetFunkyContour(distance)); // TODO: improve calling
        float funkyRot = funkyContour[1]; Logger.Log("funkyRot: " + funkyRot);
        float funkyCenter = funkyContour[2];

        float fullwidth = (TracksController.Tracks.Length * RhythmicGame.TrackWidth) / 2; // Width of TRACKS
        float pos_t = position.x / fullwidth; // -3 / 10.8 = -0.27
        float finalRot = Mathf.Lerp(-funkyRot, funkyRot, pos_t); // rot lerped by the position in TRACKS
        Quaternion funkyRot_Quat = Quaternion.Euler(new Vector3(0, 0, finalRot));

        // Rotate the right path normal
        Vector3 angleNormal = funkyRot_Quat * localRight; // Now, angleNormal is a normal pointing more downwards, potentially.
        float rightAngleDiff = Vector3.Angle(localRight, angleNormal);
        Quaternion rightAngleQuat = Quaternion.Euler(new Vector3(0, 0, rightAngleDiff));
        Vector3 rotatedUpNormal = rightAngleQuat * localUp;
        Vector3 uprightDiff = rotatedUpNormal - localUp;
        Logger.Log(uprightDiff);


        // [TEST] Draw the normal:
        Debug.DrawLine(pos, (pos + (rotatedUpNormal * 2f)), Color.red, 10000f);


        return new TrackGeo(pos, rot, funkyContour);
    }

    // TODO: add overloads without 'path'
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
        if (distance < 0f) distance = 0; // In case of negative position, zero out the distance
        else if (distance > path.length) distance = path.length - 0.01f; // HACK!!!

        return path.GetRotationAtDistance(distance) * Quaternion.Euler(0, 0, 90) * Quaternion.Euler(offset);
    }

    public static Vector3 LerpFunkyContour(Vector3[] vectors) => LerpFunkyContour(vectors[0], vectors[1]);
    public static Vector3 LerpFunkyContour(Vector3 prev, Vector3 main)
    {
        float t = prev.x / main.x; // 0.0 - 1.0

        Vector3 result = Vector3.Lerp(prev, main, t);
        result.x = t;

        return result; // x (dist) is unneccesary now        
    }
    // [dist, value, center]
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