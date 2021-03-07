using UnityEngine;
using PathCreation;
using System.Collections.Generic;

public class MeshContourEmitter : MonoBehaviour
{
    public static Mesh DeformMesh(VertexPath path, Mesh mesh, Vector3 position, float angle = 0f, Vector3[] ogVerts = null, Vector3? offset = null, float width = -1, float height = -1, float length = -1, bool movePivotToStart = false)
    {
        //Vector3[] vertices = mesh.vertices;
        // TEMP: Testing funky path contours
        Vector3[] vertices;
        if (ogVerts == null) vertices = mesh.vertices;
        else
        {
            vertices = new Vector3[ogVerts.Length];
            ogVerts.CopyTo(vertices, 0);
        }

        

        return null;
    }
    public static Mesh DeformMesh(VertexPath path, Mesh mesh, Vector3? offset = null, float width = -1, float height = -1, float length = -1, bool movePivotToStart = false) => DeformMesh(path, mesh, Vector3.zero, 0f, null, offset, width, height, length, movePivotToStart);
    public static Mesh DeformMesh(VertexPath path, Mesh mesh, Vector3 position, Vector3? offset = null, float width = -1, float height = -1, float length = -1, bool movePivotToStart = false) => DeformMesh(path, mesh, position, 0f, null, offset, width, height, length, movePivotToStart);


}