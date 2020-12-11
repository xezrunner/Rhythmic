using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using System.Linq;

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
    public static bool TestDivisibility = true;

    /// <summary>
    /// Checks whether the piece sizings are divisible.
    /// </summary>
    /// <returns>
    /// true if the piece sizings are divisible <br/>
    /// false if the piece sizings are NOT divisible
    /// </returns>
    public static bool TestPieceSizingDivisibility(float desiredX, float desiredZ, float pieceX, float pieceZ) { return (desiredX % pieceX == 0 & desiredZ % pieceZ == 0); }
    public bool TestPieceSizingDivisibility() { return TestPieceSizingDivisibility(DesiredWidth, DesiredLength, width, length); }

    public static Mesh CreateMesh(float desiredX, float desiredZ, float pieceX = 0.45f, float pieceZ = 0.5f)
    {
        if (Application.isPlaying)
        {
            //pieceX = RhythmicGame.TrackWidth / 8;
            pieceX = RhythmicGame.TrackWidth;
            pieceZ = desiredZ / 32;
        }

        //if (SongController.Instance != null)
        //    pieceZ = SongController.Instance.measureLengthInzPos / 32;

        // Test whether individual piece sizings are divisible0
        if (TestDivisibility & !TestPieceSizingDivisibility(desiredX, desiredZ, pieceX, pieceZ))
            // TODO: logging control here
            Debug.LogWarningFormat("MeshTest/CreateMesh(): Piece sizings are not divisible! (width: {0} | length: {1})", pieceX, pieceZ);

        pieceZ = desiredZ / 32;

        // Get counts for the individual pieces
        int xSize = Mathf.RoundToInt(desiredX / pieceX);
        int zSize = Mathf.RoundToInt(desiredZ / pieceZ);
        //for (float x = 0f; x < desiredX; x += pieceX) xSize++;
        //for (float z = 0f; z < desiredZ; z += pieceZ) zSize++;

        // TODO: it adds 1 extra piece for some reason...
        //if (desiredZ % pieceZ != 0)
        //zSize--;

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
