using PathCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ProBuilder;

[ExecuteInEditMode]
public class MeshDeformTest : MonoBehaviour
{
    public GameObject targetObject;
    public TrackMeshCreator pathcreator;

    [HideInInspector]
    public MeshFilter meshFilter { get { return targetObject.GetComponent<MeshFilter>(); } }
    public Mesh mesh;
    public Mesh ogMesh;
    [HideInInspector]
    public Vector3? ogPos;
    public float DesiredLength = -1f;
    VertexPath path { get { return pathcreator.path; } }

    void Awake()
    {
        GetMesh();
    }

    void GetMesh()
    {
        // Get MeshFilter from targetObject
        //meshFilter = targetObject.GetComponent<MeshFilter>();
        //if (!meshFilter) { Debug.LogErrorFormat("MeshDeformTest: The GameObject {0} does not have a MeshFilter component!", targetObject.name); return; }

        // Store both the work and og mesh variables
        ogMesh = meshFilter.sharedMesh;
        mesh = meshFilter.mesh;

        // Override ProBuilder component
        var proBuilderMesh = targetObject.GetComponent<ProBuilderMesh>();
        if (proBuilderMesh)
        {
            DestroyImmediate(targetObject.GetComponent<ProBuilderMesh>());
            DestroyImmediate(targetObject.GetComponent<MeshFilter>());
            MeshFilter newFilter = targetObject.AddComponent<MeshFilter>();
            newFilter.mesh = mesh;
        }
    }

    List<int> verticesToDrop = new List<int>();
    List<int> triangles = new List<int>();
    Vector3[] vertices;

    public void DeformMesh()
    {
        ClearAllCubes();
        //gizmosList.Clear();

        //if (mesh == null)
        //    GetMesh();
        ogMesh = meshFilter.sharedMesh;
        if (ogPos == null) ogPos = targetObject.transform.position;

        meshFilter.mesh = ogMesh;
        mesh = meshFilter.mesh;

        // Grab OG mesh vertices
        vertices = new Vector3[ogMesh.vertices.Length];
        Array.Copy(ogMesh.vertices, vertices, ogMesh.vertices.Length);

        // Transform vertices
        for (int i = 0; i < vertices.Length; i++)
        {
            //if (vertices[i].z < 0) continue;
            //else
            if (DesiredLength != -1 &&
                (vertices[i].x > path.GetPointAtDistance(DesiredLength).x || vertices[i].y > path.GetPointAtDistance(DesiredLength).y || vertices[i].z > path.GetPointAtDistance(DesiredLength).z))
            { verticesToDrop.Add(i); }

            vertices[i] = TransformVertex(vertices[i]);
        }

        triangles = mesh.triangles.ToList<int>();
        if (DesiredLength != -1f)
        {
            //List<int> triangles2 = new List<int>();

            //for (int i = 0; i < triangles.Count; i++)
            //{
            //    Vector3 result = path.GetPointAtDistance(DesiredLength);
            //    if (vertices[triangles[i]].z < result.z)
            //        triangles2.Add(triangles[i]);
            //}

            //mesh.triangles = triangles2.ToArray();
        }

        verticesToDrop.Clear();

        // Set result mesh!
        mesh.vertices = vertices;
        if (DesiredLength != -1f)
            mesh.triangles = new int[triangles.Count];
        else
            mesh.triangles = triangles.ToArray();

        // "Temporarily" set transform to 0,0,0
        targetObject.transform.position = Vector3.zero;

        if (DesiredLength != -1F)
            StreamTriangles();

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
    }

    void StreamTriangles()
    {
        List<Vector3> verts = new List<Vector3>();
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vert = vertices[i];
            if (vert.z < DesiredLength)
                verts.Add(vertices[i]);
        }

        List<int> tris = new List<int>();
        for (int i = 0; i < triangles.Count; i += 6)
            if (vertices[triangles[i]].z < DesiredLength)
            {
                // Try using index from verts list instead...
                tris.Add(triangles[i+1]);
                tris.Add(triangles[i+3]);
                tris.Add(triangles[i+2]);
                tris.Add(triangles[i + 5]);
                tris.Add(triangles[i + 4]);
                tris.Add(triangles[i]);
            }

        mesh.triangles = tris.ToArray();
    }

    void StreamTriangles_OLD()
    {
        Vector3 pathVector = ((path.GetRotationAtDistance(DesiredLength) * Quaternion.Euler(0, 0, 90)) * path.GetPointAtDistance(DesiredLength));

        // VERTS
        Vector3[] verts2 = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 meshVertex = vertices[i];

            Vector3 splinePoint = path.GetPointAtDistance(meshVertex.z + targetObject.transform.position.z);
            Vector3 futureSplinePoint = path.GetPointAtDistance(meshVertex.z + targetObject.transform.position.z + 0.01f);
            Vector3 forwardVector = futureSplinePoint - splinePoint;
            Quaternion imaginaryPlaneRotation = Quaternion.LookRotation(forwardVector, Vector3.up);
            Vector3 pointWithinPlane = new Vector3(meshVertex.x, meshVertex.y, 0f);

            Vector3 result = splinePoint + (imaginaryPlaneRotation * pointWithinPlane);

            if (Vector3.Distance(result, path.GetPointAtDistance(DesiredLength)) > DesiredLength) continue;
            verts2[i] = vertices[i];
            mesh.vertices = verts2;
        }

        // TRIS
        int[] triangles2 = new int[triangles.Count];
        for (int i = 0; i < triangles.Count; i += 1)
        {
            Vector3 meshVertex = vertices[triangles[i]];

            Vector3 splinePoint = path.GetPointAtDistance(meshVertex.z + targetObject.transform.position.z);
            Vector3 futureSplinePoint = path.GetPointAtDistance(meshVertex.z + targetObject.transform.position.z + 0.01f);
            Vector3 forwardVector = futureSplinePoint - splinePoint;
            Quaternion imaginaryPlaneRotation = Quaternion.LookRotation(forwardVector, Vector3.up);
            Vector3 pointWithinPlane = new Vector3(meshVertex.x, meshVertex.y, 0f);

            Vector3 result = splinePoint + (imaginaryPlaneRotation * pointWithinPlane);

            if (Vector3.Distance(result, path.GetPointAtDistance(DesiredLength)) > DesiredLength) continue;
            triangles2[i] = triangles[i];
            mesh.triangles = triangles2;
        }
    }

    List<GameObject> cubes = new List<GameObject>();

    public Vector3 TransformVertex(Vector3 meshVertex)
    {
        //meshVertex.z = Mathf.Abs(meshVertex.z);
        //Vector3 splinePoint = path.GetPointAtDistance(meshVertex.z);
        //Vector3 futureSplinePoint = path.GetPointAtDistance(meshVertex.z + 0.01f);
        //Vector3 forwardVector = futureSplinePoint - splinePoint;
        //Quaternion imaginaryPlaneRotation = Quaternion.LookRotation(forwardVector, Vector3.forward) * Quaternion.Euler(0, 0, 90);
        //Vector3 pointWithinPlane = new Vector3(meshVertex.x, meshVertex.y, 0f);
        //return splinePoint + (imaginaryPlaneRotation * pointWithinPlane);

        //meshVertex.z = Mathf.Abs(meshVertex.z);
        Vector3 splinePoint = path.GetPointAtDistance(meshVertex.z + targetObject.transform.position.z);
        Vector3 futureSplinePoint = path.GetPointAtDistance(meshVertex.z + targetObject.transform.position.z + 0.01f);
        Vector3 forwardVector = futureSplinePoint - splinePoint;
        Quaternion imaginaryPlaneRotation = Quaternion.LookRotation(forwardVector, Vector3.up);
        Vector3 pointWithinPlane = new Vector3(meshVertex.x, meshVertex.y, 0f);
        //Vector3 pointWithinPlane = new Vector3(0f, 0f, 0f);

        Vector3 result = splinePoint + (imaginaryPlaneRotation * pointWithinPlane);

        //var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //go.transform.parent = gameObject.transform;
        //go.transform.localScale = new Vector3(0.1f, 0.1f, 0f);
        //go.transform.position = result;
        //cubes.Add(go);

        return result;
    }

    List<Vector3[]> gizmosList = new List<Vector3[]>();

    void OnDrawGizmos()
    {
        foreach (Vector3[] v in gizmosList)
        {
            Gizmos.DrawCube(v[0], v[1]);
        }
    }

    public float Debug_Z;
    public void Debug_CreateObjectAtZ()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = path.GetPointAtDistance(Debug_Z);
        go.transform.rotation = path.GetRotationAtDistance(Debug_Z);
    }

    public void ClearAllCubes()
    {
        foreach (GameObject go in cubes) DestroyImmediate(go);
        cubes.Clear();
    }
}