using UnityEngine;
using PathCreation;

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

        // Pivot adjustment
        Vector3 pivots = MeshDeformer.GetMaxAxisValues(ref vertices);

        // Deform mesh
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x += pivots.x;
            vertices[i].y -= pivots.y;
            vertices[i].z += pivots.z;

            // We pass these values as reference so that we can change them from LerpAxis()
            MeshDeformer.LerpAxis(ref vertices[i].x, width, pivots.x, split: false, true);
            //MeshDeformer.LerpAxis(ref vertices[i].y, height, pivots.y, split: false);
            MeshDeformer.LerpAxis(ref vertices[i].z, length, pivots.z, split: false, true); // The Z axis does not require splitting here

            // Deform vertex points:
            vertices[i] = MeshDeformer.TransformVertex(path, vertices[i], position, angle, offset, length);
        }

        mesh.SetVertices(vertices);
        mesh.RecalculateBounds();

        return mesh;
    }
    public static Mesh DeformMesh(VertexPath path, Mesh mesh, Vector3? offset = null, float width = -1, float height = -1, float length = -1, bool movePivotToStart = false) => DeformMesh(path, mesh, Vector3.zero, 0f, null, offset, width, height, length, movePivotToStart);
    public static Mesh DeformMesh(VertexPath path, Mesh mesh, Vector3 position, Vector3? offset = null, float width = -1, float height = -1, float length = -1, bool movePivotToStart = false) => DeformMesh(path, mesh, position, 0f, null, offset, width, height, length, movePivotToStart);


}