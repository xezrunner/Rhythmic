using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeshMaxAxisValueGetter))]
public class MeshMaxAxisValueGetterEditor : Editor
{
    MeshMaxAxisValueGetter main;
    private void Awake() => main = (MeshMaxAxisValueGetter)target;

    public override void OnInspectorGUI()
    {
        Mesh mesh = main.Mesh;
        if (mesh != null)
        {
            float[] maxValues = MeshDeformer.GetMaxAxisValues(mesh.vertices);

            GUILayout.Label("Mesh max axis values: \n" +
                            $"X: {maxValues[0]}\n" +
                            $"Y: {maxValues[1]}\n" +
                            $"Z: {maxValues[2]}");

            GUILayout.BeginHorizontal();

            GUILayout.Label("X: ");
            GUILayout.TextField(maxValues[0].ToString());

            GUILayout.Label("Y: ");
            GUILayout.TextField(maxValues[1].ToString());

            GUILayout.Label("Z: ");
            GUILayout.TextField(maxValues[2].ToString());

            GUILayout.EndHorizontal();
        }
    }
}
