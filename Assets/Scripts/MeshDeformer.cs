using PathCreation;
using System.Linq;
using UnityEngine;

public enum Axis { X = 0, Y = 1, Z = 2, W = 3 }

public static class MeshDeformer
{
    /// Mesh deformation utilities

    // TODO: improve argument naming? Mostly width, height and length
    /// <summary>
    /// Deforms a mesh's vertices on a path. <br/> <br/>
    /// <paramref name="width"/> (X), <paramref name="height"/> (Y) and <paramref name="length"/> (Z) can be passed to scale the mesh on these axes before being deformed. <br/>
    /// Note: no new vertices will be created - existing vertices will be stretched on their axes!
    /// </summary>
    /// <param name="offset">
    /// The offset to use during path position calculation. <br/>
    /// This will offset the mesh vertices on the path using the path normals at the given distance. ('parallel offsetting')
    /// </param>
    /// <param name="movePivotToStart">
    /// Whether the mesh vertices should be displaced on the Z axis so that the mesh starts at its -Z axis edge. <br/>
    /// Useful for track-related models that don't already have their position pivoted to the -Z edge.
    /// </param>
    public static Mesh DeformMesh(VertexPath path, Mesh mesh, Vector3 position, float angle = 0f, Vector3[] ogVerts = null, Vector3? offset = null, float width = -1, float height = -1, float length = -1, bool movePivotToStart = false)
    {
        Vector3[] vertices = mesh.vertices;

        // Pivot adjustment on Z axis
        float p_maxZ = movePivotToStart ? GetMaxAxisValue(vertices, Axis.Z) : 0;

        // XYZ adjustments
        bool isXYZAdjustment = (width + height + length != -3); // Whether at least one of the XYZ values is adjusted
        float maxX = -1, maxY = -1, maxZ = -1;

        if (isXYZAdjustment)
        {
            if (width != -1) maxX = GetMaxAxisValue(vertices, Axis.X);
            if (height != -1) maxY = GetMaxAxisValue(vertices, Axis.Y);
            if (length != -1) maxZ = GetMaxAxisValue(vertices, Axis.Z);
        }

        // Deform mesh
        for (int i = 0; i < vertices.Length; i++)
        {
            // Adjust Z pivot to the mesh starting point:
            vertices[i].z += p_maxZ;

            // XYZ adjustments processing:
            if (isXYZAdjustment)
            {
                // We pass these values as reference so that we can change them from LerpAxis()
                if (maxX != -1) LerpAxis(ref vertices[i].x, width, maxX);
                if (maxY != -1) LerpAxis(ref vertices[i].y, height, maxY);
                if (maxZ != -1) LerpAxis(ref vertices[i].z, length, maxZ, split: false); // The Z axis does not require splitting here
            }

            // Deform vertex points:
            vertices[i] = TransformVertex(path, vertices[i], position, angle, offset, length);
        }

        mesh.SetVertices(vertices);
        mesh.RecalculateBounds();

        return mesh;
    }
    public static Mesh DeformMesh(VertexPath path, Mesh mesh, Vector3? offset = null, float width = -1, float height = -1, float length = -1, bool movePivotToStart = false) => DeformMesh(path, mesh, Vector3.zero, 0f, null, offset, width, height, length, movePivotToStart);
    public static Mesh DeformMesh(VertexPath path, Mesh mesh, Vector3 position, Vector3? offset = null, float width = -1, float height = -1, float length = -1, bool movePivotToStart = false) => DeformMesh(path, mesh, position, 0f, null, offset, width, height, length, movePivotToStart);

    /// <summary>
    /// The result is: <br />
    /// • Point along the path <br />
    /// + the rotation along the path <br />
    /// * the current vertex point's X and Y coords <br />
    /// In laymen's terms: the point and rotation along the path + the X and Y being offset with the path's rotation in mind
    /// </summary>
    public static Vector3 TransformVertex(VertexPath path, Vector3 meshVertex, Vector3 position, float angle, Vector3? offset, float length = 0f)
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

    // Axis utilities:
    // TODO: Some of these should be generalized to support Vector2/3/4s as well as Quaternions.
    // VectorUtilities or some kind of other utility class that could contain these and more?

    /// <summary>
    /// Lerps towards a target axis value. <br/>
    /// It splits <paramref name="target"/> into half and determines the result (for both negative and positive sides) from the given
    /// <paramref name="value"/>, turned into a fraction by <paramref name="valueMax"/>.
    /// </summary>
    /// <param name="value">The value to lerp.</param>
    /// <param name="target">The target value you want to lerp to.</param>
    /// <param name="valueMax">Used to determine the fraction value t from <paramref name="value"/>.</param>
    static float LerpAxis(float value, float target, float valueMax, bool split = true) { return LerpAxis(ref value, target, valueMax, split); } // TODO: is this legal? Will the ref here affect the original variable?
    static float LerpAxis(ref float value, float target, float valueMax, bool split = true)
    {
        float t = Mathf.Abs(value / valueMax);
        float result;

        if (split)
            target /= 2; // Split target value into 2

        // Since Mathf.Lerp()'s t parameter does not support negative numbers, we have to swap the targets
        // and use the absolute value as t.
        result = Mathf.Lerp(0, (value < 0f) ? -target : target, t);

        value = result; // Change ref value to result
        return result;
    }

    /// <summary>
    /// Returns the maximum value of a Vector3 component.
    /// </summary>
    /// <param name="axis">The desired Vector3 component to get the maximum value of</param>
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

    /// <summary>
    /// Returns an array of 3 float values for all of a Vector3's components' max values.
    /// </summary>
    public static float[] GetMaxAxisValues(Vector3[] vertices) // TODO: copying the vertices - this is baaad!
    {
        float[] result = new float[3];

        for (int i = 0; i < 3; i++)
            result[i] = GetMaxAxisValue(vertices, (Axis)i);

        return result;
    }
    //public static float[] GetMaxAxisValues(Mesh mesh) => GetMaxAxisValues(mesh.vertices);
}