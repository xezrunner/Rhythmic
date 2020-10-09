using PathCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        Vector3[] vertices = new Vector3[ogMesh.vertices.Length];
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

        if (DesiredLength != -1 || DesiredLength != 0)
        {
            List<int> triangles = mesh.triangles.ToList<int>();
            List<int> triangles2 = new List<int>();

            /*
            for (int i = 0; i < vertices.Length; i += 3)
            {
                if (vertices[triangles[i]].x < path.GetPointAtDistance(DesiredLength).z &
                    vertices[triangles[i + 1]].x < path.GetPointAtDistance(DesiredLength).z &
                    vertices[triangles[i + 2]].x < path.GetPointAtDistance(DesiredLength).z)
                {
                    triangles2.Add(triangles[i]);
                    triangles2.Add(triangles[i + 1]);
                    triangles2.Add(triangles[i + 2]);
                    triangles2.Add(triangles[i + 3]);
                    triangles2.Add(triangles[i + 4]);
                    triangles2.Add(triangles[i + 5]);
                }
                //if (vertices[triangles[i + 2]].z < DesiredLength) triangles2.Add(i + 2);
                //if (vertices[triangles[i + 1]].z < DesiredLength) triangles2.Add(i + 1);
                //if (vertices[triangles[i]].z < DesiredLength) triangles2.Add(i);
            }
            */
            for (int i = 0; i < triangles.Count; i++)
            {
                if (vertices[triangles[i]].z > path.GetPointAtDistance(DesiredLength).z)
                {
                    //vertices[triangles[i]].z = path.GetPointAtDistance(DesiredLength).z;
                    //triangles2.Add(triangles[i]);
                }
            }

            //mesh.triangles = triangles2.ToArray();
        }

        verticesToDrop.Clear();

        // Set result mesh!
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        // "Temporarily" set transform to 0,0,0
        targetObject.transform.position = Vector3.zero;
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
