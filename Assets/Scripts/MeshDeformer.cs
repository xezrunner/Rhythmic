using PathCreation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class MeshDeformer
{
    // Deforms the parameter mesh's vertices
    public static void DeformMesh(VertexPath path, Mesh mesh) => DeformMesh(path, mesh, Vector3.zero, Vector3.zero);
    public static void DeformMesh(VertexPath path, Mesh mesh, Vector3 position) => DeformMesh(path, mesh, position, Vector3.zero);
    //public static void DeformMesh(VertexPath path, Mesh mesh, Vector3 position, Vector3 rotation, Vector3[] ogVerts = null)
    //{
    //    Vector3[] vertices = ogVerts != null ? ogVerts : mesh.vertices;

    //    // Transform mesh vertices
    //    for (int i = 0; i < mesh.vertices.Length; i++)
    //    {
    //        Vector3 vertex = vertices[i];
    //        vertices[i] = TransformVertex(path, vertex, position, rotation);
    //    }

    //    mesh.vertices = vertices;
    //    mesh.RecalculateBounds();
    //}

    public static async void DeformMesh(VertexPath path, Mesh mesh, Vector3 position, Vector3 rotation, Vector3[] ogVerts = null)
    {
        Vector3[] vertices = ogVerts != null ? ogVerts : mesh.vertices;

        // Transform mesh vertices
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            vertices[i] = TransformVertex(path, vertex, position, rotation);
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
    }

    /// <summary>
    /// The result is: <br />
    /// • Point along the path <br />
    /// + the rotation along the path <br />
    /// * the current vertex point's X and Y coords <br />
    /// In laymen's terms: the point and rotation along the path + the X and Y being offset with the path's rotation in mind
    /// </summary>
    public static Vector3 TransformVertex(VertexPath path, Vector3 meshVertex, Vector3 position, Vector3 rotation)
    {
        float dist = meshVertex.z + position.z; // Vertex Z distance along path + desired Z offset
        Vector3 localUp = Vector3.Cross(path.GetTangentAtDistance(dist), path.GetNormalAtDistance(dist));
        Vector3 localRight = path.GetNormalAtDistance(dist);

        Vector3 splinePoint = path.GetPointAtDistance(dist); // Point on path at the distance
        splinePoint += (localRight * position.x) + (localUp * position.y); // Parallel position offsetting

        Quaternion pathRotation = path.GetRotationAtDistance(dist) * Quaternion.Euler(0, 0, 90); // Rot on path at the distance
        pathRotation = pathRotation * Quaternion.Euler(rotation); // Rotation addition

        Vector3 vertexXY = new Vector3(meshVertex.x, meshVertex.y, 0f); // Vertex X and Y points (horizontal)

        return splinePoint + (pathRotation * vertexXY);
    }
}
