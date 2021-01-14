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

        // Transform mesh vertices
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = TransformVertex(path, vertices[i], position, angle, offset);

        mesh.vertices = vertices;
        mesh.RecalculateBounds();

        return mesh;
    }

    /// <summary>
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