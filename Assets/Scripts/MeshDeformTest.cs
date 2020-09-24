using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDeformTest : MonoBehaviour
{
    public GameObject targetObject;
    public TrackMeshCreator pathcreator;

    MeshFilter meshFilter;
    public Mesh mesh;
    Mesh ogMesh;
    VertexPath path { get { return pathcreator.path; } }

    void Awake()
    {
        GetMesh();
    }

    void GetMesh()
    {
        // Get MeshFilter from targetObject
        meshFilter = targetObject.GetComponent<MeshFilter>();
        if (!meshFilter) { Debug.LogErrorFormat("MeshDeformTest: The GameObject {0} does not have a MeshFilter component!", targetObject.name); return; }

        // Store both the work and og mesh variables
        mesh = ogMesh = meshFilter.mesh;
    }

    public void DeformMesh()
    {
        gizmosList.Clear();
        GetMesh();

        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = TransformVertex(vertices[i]);

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    public Vector3 TransformVertex(Vector3 meshVertex)
    {
        Vector3 splinePoint = path.GetPointAtDistance(meshVertex.z);
        Vector3 futureSplinePoint = path.GetPointAtDistance(meshVertex.z + 0.01f);
        Vector3 forwardVector = futureSplinePoint - splinePoint;
        Quaternion imaginaryPlaneRotation = Quaternion.LookRotation(forwardVector, Vector3.up);
        Vector3 pointWithinPlane = new Vector3(meshVertex.x, meshVertex.y, 0f);

        return splinePoint + imaginaryPlaneRotation * pointWithinPlane;
    }

    List<Vector3[]> gizmosList = new List<Vector3[]>();

    void OnDrawGizmos()
    {
        foreach (Vector3[] v in gizmosList)
        {
            Gizmos.DrawCube(v[0], v[1]);
        }
    }
}
