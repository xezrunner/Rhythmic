using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldSystem))]
class WorldSystemEditor : Editor
{
    float dist;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        dist = EditorGUILayout.FloatField(dist);
        if (GUILayout.Button("GetFunkyContour()"))
        {
            Vector3[] v = PathTools.GetFunkyContour(dist);
            Logger.Log("distance param: " + $"{dist}\n".AddColor(Colors.Error) +
                       "v0: " + $"dist: {v[0].x}, value: {v[0].y}, center: {v[0].z}\n".AddColor(Colors.Error) +
                       "v1: " + $"dist: {v[1].x}, value: {v[1].y}, center: {v[1].z}".AddColor(Colors.Error));
        }
    }
}