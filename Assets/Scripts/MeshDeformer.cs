using PathCreation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class MeshDeformer
{
    // Deforms the parameter mesh's vertices
    public static Mesh DeformMesh(VertexPath path, Mesh mesh) => DeformMesh(path, mesh, Vector3.zero);
    public static Mesh DeformMesh(VertexPath path, Mesh mesh, Vector3 position) => DeformMesh(path, mesh, position);
    public static Mesh DeformMesh(VertexPath path, Mesh mesh, Vector3 position, float angle = 0f, Vector3[] ogVerts = null)
    {
        Vector3[] vertices = ogVerts != null ? ogVerts : new Vector3[mesh.vertices.Length];
        if (ogVerts == null) Array.Copy(mesh.vertices, vertices, mesh.vertices.Length);

        // Transform mesh vertices
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = TransformVertex(path, vertices[i], position, angle);

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
    public static Vector3 TransformVertex(VertexPath path, Vector3 meshVertex, Vector3 position, float angle)
    {
        float dist = meshVertex.z + position.z; // Vertex Z distance along path + desired Z offset
        bool isNegative = dist < 0f; // TODO: negative path values should just over/under-flow the path! Right now, it wraps around the path.

        //if (dist < path.localPoints[0].z)
        //{
        //    Vector3 pos = new Vector3(0,0, dist) + (Quaternion.Euler(0,0,180) * new Vector3(meshVertex.x, meshVertex.y, 0));
        //    pos += Vector3.right * position.x;

        //    return pos;
        //}

        //Vector3 localUp = Vector3.Cross(path.GetTangentAtDistance(dist), path.GetNormalAtDistance(dist));
        //Vector3 localRight = path.GetNormalAtDistance(dist);

        //Vector3 splinePoint = path.GetPointAtDistance(dist); // Point on path at the distance
        //splinePoint += (localRight * position.x) + (localUp * position.y); // Parallel position offsetting

        Vector3 splinePoint = PathTools.GetPositionOnPath(path, dist, position - Tunnel.Instance.center);

        Quaternion pathRotation = path.GetRotationAtDistance(dist) * Quaternion.Euler(0, 0, 90); // Rot on path at the distance
        pathRotation = pathRotation * Quaternion.Euler(0, 0, angle); // Rotation addition

        Vector3 vertexXY = new Vector3(meshVertex.x, meshVertex.y, 0f); // Vertex X and Y points (horizontal)

        return splinePoint + (pathRotation * vertexXY);
    }
}