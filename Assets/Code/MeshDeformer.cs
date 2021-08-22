using PathCreation;
using System.Collections.Generic;
using UnityEngine;
using static Logger;

public class MeshDeformer : MonoBehaviour
{
    public static MeshDeformer Instance;

    //public PathCreator pathcreator;
    //VertexPath path;

    public bool PATH_Multithreading = false; // TODO

    void Awake()
    {
        Instance = this;

        //if (!pathcreator) pathcreator = FindObjectOfType<PathCreator>();
        //if (!pathcreator)
        //{ LogE("No pathcreator was set! Deforms will fail.".T(this)); return; }
        //
        //path = pathcreator.path;
    }

#if false
    public void DeformMesh(ref Mesh mesh) // Meshes are passed as a reference.
    {
        int count = mesh.vertexCount;

        List<Vector3> verts = new List<Vector3>(count);
        mesh.GetVertices(verts);

        for (int i = 0; i < count; ++i)
        {
            Vector3 v = verts[i];
            Vector3 v_nonz = new Vector3(v.x, v.y, 0);
            float dist = v.z + 20f;

            Vector3 p_point = path.GetPointAtDistance(dist);
            Quaternion p_rot = path.GetRotationAtDistance(dist);

            Vector3 final = p_point + (p_rot * v_nonz);
            verts[i] = final;
        }

        mesh.SetVertices(verts);
        mesh.RecalculateBounds();
    }
#endif


}

