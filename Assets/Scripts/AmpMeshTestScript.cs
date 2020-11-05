using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class AmpMeshTestScript : MonoBehaviour
{
    /// Editor functionality
    #region Editor functionality
    public GameObject lastGo;

    [Header("Individual unit sizes")]
    public float width = 0.45f;
    public float length = 0.45f;

    [Header("Desired width & length")]
    public float DesiredWidth = 3.6f;
    public float DesiredLength = 32f;

    [Header("Other properties")]
    public float xPosition = 0f;
    public float yElevation = 0f;

    public GameObject CreateGameObject()
    {
        Mesh mesh = CreateMesh_Editor();

        var go = new GameObject();
        var meshFilter = go.AddComponent<MeshFilter>();
        var meshRenderer = go.AddComponent<MeshRenderer>();

        // Assign mesh
        meshFilter.sharedMesh = mesh;

        lastGo = go;
        return go;
    }
    public void UpdateGameObject()
    {
        if (!lastGo) { Debug.LogError("MeshTest: lastGo is null!"); return; }
        lastGo.GetComponent<MeshFilter>().mesh = CreateMesh_Editor();
    }

    public bool TestDivisibility_Editor = true;
    public Mesh CreateMesh_Editor() { return CreateMesh(DesiredWidth, DesiredLength, width, length); }
    #endregion

    /// Standalone functionality
    /// TODO:
        /// Optimize nearest division in some way
        /// Right now, nearest division correction takes time for the CPU to perform every time.
        /// Possible solutions: 
            /// - caching the value
            /// - using a (default) global static value and correcting only once [ Seems more favorable ]

    public static bool TestDivisibility = true;

    /// <summary>
    /// Finds the nearest number to divide the sizing pieces by.
    /// If it can't find one, it'll reset the original numbers and continue working as normal.
    /// </summary>
    public static void FindNearestDivisionForPieces(float desiredX, float desiredZ, ref float pieceX, ref float pieceZ)
    {
        float ogWidth = pieceX; float ogLength = pieceZ;

        var watch = new System.Diagnostics.Stopwatch(); watch.Start();
        while (desiredX % pieceX != 0 || pieceX == 1)
        {
            if (pieceX - 0.01f > 0) pieceX = (float)Math.Round(pieceX - 0.01, 2);
            else pieceX = (float)Math.Round(pieceX + 0.01, 2);

            if (watch.ElapsedMilliseconds > 1000f) { Debug.Log("MeshTest: failed to find nearest division!"); pieceX = ogWidth; break; }
        }

        while (desiredZ % pieceZ != 0 || pieceZ == 1)
        {
            if (pieceZ - 0.01f > 0) pieceZ = (float)Math.Round(pieceZ - 0.01, 2);
            else pieceZ = (float)Math.Round(pieceZ + 0.01, 2);

            if (watch.ElapsedMilliseconds > 1000f) { Debug.Log("MeshTest: failed to find nearest division!"); pieceZ = ogLength; break; }
        }

        watch.Stop();
    }
    public void FindNearestDivisionForPieces() => FindNearestDivisionForPieces(DesiredWidth, DesiredLength, ref width, ref length);

    /// <summary>
    /// Checks whether the piece sizings are divisible.
    /// </summary>
    /// <returns>
    /// true if the piece sizings are divisible <br/>
    /// false if the piece sizings are NOT divisible
    /// </returns>
    public static bool TestPieceSizingDivisibility(float desiredX, float desiredZ, float pieceX, float pieceZ) { return (desiredX % pieceX == 0 & desiredZ % pieceZ == 0); }
    public bool TestPieceSizingDivisibility() { return TestPieceSizingDivisibility(DesiredWidth, DesiredLength, width, length); }
    public static Mesh CreateMesh(float desiredX, float desiredZ, float pieceX = 0.45f, float pieceZ = 0.45f)
    {
        // Test whether individual piece sizings are divisible0
        if (TestDivisibility & !TestPieceSizingDivisibility(desiredX, desiredZ, pieceX, pieceZ))
        {
            // TODO: logging control here
            Debug.LogWarningFormat("MeshTest/CreateMesh(): Piece sizings are not divisible! (width: {0} | length: {1})", pieceX, pieceZ);
            FindNearestDivisionForPieces(desiredX, desiredZ, ref pieceX, ref pieceZ);
        }

        // Get counts for the individual pieces
        int xSize = 0;
        int zSize = 0;
        for (float x = 0f; x < desiredX; x += pieceX) xSize++;
        for (float z = 0f; z < desiredZ; z += pieceZ) zSize++;

        Mesh mesh = new Mesh() { name = "Procedurally generated mesh" };

        Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        int[] triangles = new int[xSize * zSize * 6];
        Vector2[] uv = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];

        // Vertices:
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                float xValue = x * pieceX - desiredX / 2; // Center X
                float zValue = z * pieceZ;
                vertices[i] = new Vector3(xValue, 0, zValue);
                uv[i] = new Vector2((float)x / xSize, (float)z / xSize);
                tangents[i] = new Vector4(1f, 0f, 0f, -1f);
            }
        }

        // Triangles:
        for (int ti = 0, vi = 0, y = 0; y < zSize; y++, vi++)
        {
            for (int x = 0; x < xSize; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                triangles[ti + 5] = vi + xSize + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.tangents = tangents;
        mesh.RecalculateNormals();

        return mesh;
    }

}
