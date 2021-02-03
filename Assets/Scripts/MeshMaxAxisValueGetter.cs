using UnityEngine;

public class MeshMaxAxisValueGetter : MonoBehaviour
{
    public Mesh Mesh
    {
        get
        {
            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter) return meshFilter.mesh;
            else if (meshFilter.mesh is null)
            {
                Debug.LogError($"{nameof(MeshMaxAxisValueGetter)}: MeshFilter.mesh is null!!");
                return null;
            }
            else
            {
                Debug.LogError($"{nameof(MeshMaxAxisValueGetter)}: No MeshFilter attached!");
                return null;
            }
        }
    }
}
