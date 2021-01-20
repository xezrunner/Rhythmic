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
        if (!targetObject) return;

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

        MeshDeformer.DeformMesh(path, meshFilter.mesh, targetObject.transform.position, targetObject.transform.position.x);

        targetObject.transform.position = Vector3.zero;

        return;

        // Grab OG mesh vertices
        vertices = new Vector3[ogMesh.vertices.Length];
        Array.Copy(ogMesh.vertices, vertices, ogMesh.vertices.Length);

        // Transform vertices
        for (int i = 0; i < vertices.Length; i++)
        {
            //if (vertices[i].z < 0) continue;
            //else
            /*
            if (DesiredLength != -1 &&
                (vertices[i].x > path.GetPointAtDistance(DesiredLength).x || vertices[i].y > path.GetPointAtDistance(DesiredLength).y || vertices[i].z > path.GetPointAtDistance(DesiredLength).z))
            { verticesToDrop.Add(i); }
            */

            vertices[i] = TransformVertex(vertices[i]);
        }

        triangles = mesh.triangles.ToList<int>();
        if (DesiredLength != -1f)
        {


        }

        verticesToDrop.Clear();

        // Set result mesh!
        mesh.vertices = vertices;
        if (DesiredLength != -1f) { }
        //mesh.triangles = new int[triangles.Count];
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

    public Transform Cube;

    public Vector3 GetVectorCustom(Vector3 point)
    {
        Vector3 splinePoint = path.GetPointAtDistance(point.z + targetObject.transform.position.z);
        Vector3 futureSplinePoint = path.GetPointAtDistance(point.z + targetObject.transform.position.z + 0.01f);
        Vector3 forwardVector = futureSplinePoint - splinePoint;
        Quaternion imaginaryPlaneRotation = Quaternion.LookRotation(forwardVector, Vector3.up);
        Vector3 pointWithinPlane = new Vector3(point.x, point.y, 0f);
        //Vector3 pointWithinPlane = new Vector3(0f, 0f, 0f);

        Vector3 result = splinePoint + (imaginaryPlaneRotation * pointWithinPlane);
        return result;
    }

    public GameObject container;
    List<GameObject> averageSphereList = new List<GameObject>();

    void StreamTriangles()
    {
        List<int> tris = new List<int>();
        foreach (GameObject o in averageSphereList)
            DestroyImmediate(o);
        averageSphereList.Clear();

        // Get cutoff point
        Vector3 cutoff = path.GetPointAtDistance(ogPos.Value.z + DesiredLength /* - 1 */);
        //Vector3 cutoff = GetVectorCustom(new Vector3(0, 0, ogPos.Value.z + DesiredLength));

        for (int t = 0; t < triangles.Count; t += 3)
        {
            // Calculate the average vector for 3 triangles
            Vector3 average = Vector3.zero;
            Vector3 tempVector = Vector3.zero;
            for (int i = 0; i < 3; i++)
                tempVector += vertices[triangles[t + i]];
            average = tempVector / 3;

            // Calculate the distance between the triangle average and the cutoff point
            float distance = Vector3.Distance(average, cutoff);
            //distance = (float)System.Math.Round(distance, 2, MidpointRounding.AwayFromZero);

            //float distance = average.z - cutoff.z;

            // If we're behind the cutoff point, add triangles
            //if (distance < DesiredLength)
            if (average.z < cutoff.z)
            {
                for (int i = 0; i < 3; i++)
                    tris.Add(triangles[t + i]);

                for (int i = 0; i < 3; i++)
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    go.transform.position = vertices[triangles[t + i]];
                    go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    go.transform.parent = container.transform;
                    averageSphereList.Add(go);
                }

                /*
                    var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.position = average;
                go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                go.transform.parent = container.transform;
                averageSphereList.Add(go);
                */
            }


            //Debug.Log(distance);
            //if (Cube)
            //    Cube.transform.position = average;

            Cube.transform.position = cutoff;
            Cube.transform.rotation = path.GetRotationAtDistance(ogPos.Value.z + DesiredLength);

            //await Task.Delay(1);
        }
        mesh.triangles = tris.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        /*
        if (tris.Count % 3 != 0)
            for (int i = 0; i < tris.Count % 3; i++)
                tris.Add(0);
        */

        /* ----- */

        /*
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
        */
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
        float dist = meshVertex.z + targetObject.transform.position.z; // Vertex Z distance along path + desired Z offset
        Vector3 localUp = Vector3.Cross(path.GetTangentAtDistance(dist), path.GetNormalAtDistance(dist));
        Vector3 localRight = path.GetNormalAtDistance(dist);

        Vector3 splinePoint = path.GetPointAtDistance(dist); // Point on path at the distance
        splinePoint += (localRight * targetObject.transform.position.x) + (localUp * targetObject.transform.position.y); // Parallel offsetting

        Quaternion pathRotation = path.GetRotationAtDistance(dist) * Quaternion.Euler(0, 0, 90); // Rot on path at the distance
        Vector3 vertexHorizontal = new Vector3(meshVertex.x, meshVertex.y, 0f); // Vertex X and Y points (horizontal)

        return splinePoint + (pathRotation * vertexHorizontal);
    }

    //public Vector3 TransformVertex(Vector3 meshVertex)
    //{
    //    float dist = meshVertex.z + targetObject.transform.position.z;

    //    Vector3 localUp = Vector3.Cross(path.GetTangentAtDistance(dist), path.GetNormalAtDistance(dist));
    //    Vector3 localRight = path.GetNormalAtDistance(dist);

    //    Vector3 splinePoint = path.GetPointAtDistance(dist);
    //    Vector3 futureSplinePoint = path.GetPointAtDistance(dist + 0.01f);

    //    splinePoint += (localRight * targetObject.transform.position.x) + (localUp * targetObject.transform.position.y);
    //    futureSplinePoint += (localRight * targetObject.transform.position.x) + (localUp * targetObject.transform.position.y);

    //    Vector3 forwardVector = futureSplinePoint - splinePoint;
    //    Quaternion imaginaryPlaneRotation = path.GetRotationAtDistance(dist) * Quaternion.Euler(0, 0, 90);
    //    Vector3 pointWithinPlane = new Vector3(meshVertex.x, meshVertex.y, 0f);

    //    Vector3 result = splinePoint + (imaginaryPlaneRotation * pointWithinPlane);

    //    return result;
    //}

    //public Vector3 TransformVertex(Vector3 meshVertex)
    //{
    //    Vector3 splinePoint = path.GetPointAtDistance(meshVertex.z + targetObject.transform.position.z);
    //    Vector3 futureSplinePoint = path.GetPointAtDistance(meshVertex.z + targetObject.transform.position.z + 0.01f);
    //    Vector3 forwardVector = futureSplinePoint - splinePoint;
    //    Quaternion imaginaryPlaneRotation = Quaternion.LookRotation(forwardVector, Vector3.up);
    //    Vector3 pointWithinPlane = new Vector3(meshVertex.x, meshVertex.y, 0f);

    //    Vector3 result = splinePoint + (imaginaryPlaneRotation * pointWithinPlane);
    //    //result += Vector3.right * targetObject.transform.position.x;
    //    //result += Vector3.up * targetObject.transform.position.y;

    //    return result;
    //}

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