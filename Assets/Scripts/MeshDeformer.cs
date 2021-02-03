using PathCreation;
using System;
using UnityEngine;

public static class MeshDeformer
{
    // Deforms the parameter mesh's vertices
    public static Mesh DeformMesh(VertexPath path, Mesh mesh, Vector3? offset = null) => DeformMesh(path, mesh, Vector3.zero, 0f, null, offset);
    public static Mesh DeformMesh(VertexPath path, Mesh mesh, Vector3 position, Vector3? offset = null) => DeformMesh(path, mesh, position, 0f, null, offset);
    public static Mesh DeformMesh(VertexPath path, Mesh mesh, Vector3 position, float angle = 0f, Vector3[] ogVerts = null, Vector3? offset = null)
    {
        Vector3[] vertices = ogVerts != null ? ogVerts : new Vector3[mesh.vertices.Length];
        if (ogVerts == null) Array.Copy(mesh.vertices, vertices, mesh.vertices.Length);

<<<<<<< Updated upstream
        // Transform mesh vertices
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = TransformVertex(path, vertices[i], position, angle, offset);
=======
		// Pivot adjustment to start
        float p_maxZ = 0;
        if (movePivotToStart)
            p_maxZ = GetMaxAxisValue(vertices, Axis.Z);

        // XYZ adjustments
        bool isXYZAdjustment = (width + height + length != -3); // Whether XYZ is adjusted at all
        float maxX = -1, maxY = -1, maxZ = -1;

        // XYZ adjustments setup:
        if (isXYZAdjustment)
        {
            maxX = width != -1 ? GetMaxAxisValue(vertices, Axis.X) : width;
            maxY = height != -1 ? GetMaxAxisValue(vertices, Axis.Y) : height;
            maxZ = length != -1 ? GetMaxAxisValue(vertices, Axis.Z) : length;
        }

        // Deform mesh
        for (int i = 0; i < vertices.Length; i++)
        {
            // Adjust Z pivot to the mesh starting point:
            if (movePivotToStart)
                vertices[i].z += p_maxZ;

            // XYZ adjustments:
            if (isXYZAdjustment)
            {
                if (maxX != -1) LerpAxis(ref vertices[i].x, width, maxX);
                if (maxY != -1) LerpAxis(ref vertices[i].y, height, maxY);
                if (maxZ != -1) LerpAxis(ref vertices[i].z, length, maxZ);
            }

            // Deform vertex points:
            vertices[i] = TransformVertex(path, vertices[i], position, angle, offset, length);
        }
>>>>>>> Stashed changes

        mesh.vertices = vertices;
        mesh.RecalculateBounds();

        return mesh;
    }

    /// <summary>
<<<<<<< Updated upstream
=======
    /// Lerps towards a target axis value. <br/>
    /// It splits <paramref name="target"/> into half and determines the result (for both negative and positive sides) from the given
    /// <paramref name="value"/>, turned into a fraction by <paramref name="valueMax"/>.
    /// </summary>
    /// <param name="value">The value to lerp.</param>
    /// <param name="target">The target value you want to lerp to.</param>
    /// <param name="valueMax">Used to determine the fraction value t from <paramref name="value"/>.</param>
    /// <returns></returns>
    static float LerpAxis(ref float value, float target, float valueMax)
    {
        float t = Mathf.Abs(value / valueMax);
        float result;

        target /= 2; // Split target value into 2

        // Since Mathf.Lerp()'s t parameter does not support negative numbers, we have to swap the targets
        // and use absolute value as t.
        if (value < 0f)
            result = Mathf.Lerp(0, -target, t);
        else
            result = Mathf.Lerp(0, target, t);

        value = result; // Change ref value to result
        return result;
    }

	// TODO: Generalize these for Vector2/3/4 & Quaternions?
    public static float GetMaxAxisValue(Mesh mesh, Axis axis) => GetMaxAxisValue(mesh.vertices, axis);
    public static float GetMaxAxisValue(Vector3[] vertices, Axis axis)
    {
        switch (axis)
        {
            case Axis.X: return vertices.Max(v => v.x);
            case Axis.Y: return vertices.Max(v => v.y);
            case Axis.Z: return vertices.Max(v => v.z);
            case Axis.W:
                Debug.LogError("GetMaxAxisValue(): W is not a valid axis for vertices!");
                return 0;

            default: return 0;
        }
    }

    public static float[] GetMaxAxisValues(Mesh mesh) => GetMaxAxisValues(mesh.vertices);
    public static float[] GetMaxAxisValues(Vector3[] vertices)
    {
        float[] result = new float[3];

        for (int i = 0; i < 3; i++)
            result[i] = GetMaxAxisValue(vertices, (Axis)i);

        return result;
    }

    /// <summary>
>>>>>>> Stashed changes
    /// The result is: <br />
    /// • Point along the path <br />
    /// + the rotation along the path <br />
    /// * the current vertex point's X and Y coords <br />
    /// In laymen's terms: the point and rotation along the path + the X and Y being offset with the path's rotation in mind
    /// </summary>
    public static Vector3 TransformVertex(VertexPath path, Vector3 meshVertex, Vector3 position, float angle, Vector3? offset)
    {
        Vector3 tunnelCenter = (Tunnel.Instance) ? Tunnel.Instance.center : Vector3.zero;

        float distance = meshVertex.z + position.z; // Vertex Z distance along path + desired Z offset
        Vector3 vertexXY = new Vector3(meshVertex.x, meshVertex.y, 0); // Vertex X and Y points (horizontal)
        Vector3 pointOnPath = PathTools.GetPositionOnPath(path, distance, position - tunnelCenter);
        Quaternion pathRotation = PathTools.GetRotationOnPath(path, distance); // Rot on path at the distance

        pathRotation = pathRotation * Quaternion.Euler(0, 0, angle); // Offset rotation

        Vector3 final = pointOnPath + (pathRotation * vertexXY);
        if (offset.HasValue) final += pathRotation * offset.Value; // TODO: might not be neccessary

        return final;
    }
}