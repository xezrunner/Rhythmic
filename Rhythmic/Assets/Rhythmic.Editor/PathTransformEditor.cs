using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathTransform))]
[CanEditMultipleObjects]
public class PathTransformEditor : Editor
{
    PathTransform main;
    void Awake() => main = (PathTransform)target;

    SerializedProperty pos;
    SerializedProperty rot;
    SerializedProperty desired_size;

    void OnEnable()
    {
        pos = serializedObject.FindProperty("pos");
        rot = serializedObject.FindProperty("euler_rot");
        desired_size = serializedObject.FindProperty("desired_size");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(pos);
        EditorGUILayout.PropertyField(rot);
        EditorGUILayout.PropertyField(desired_size);

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();

        EditorGUILayout.LabelField($"Max values: {main.max_values}");
        EditorGUILayout.LabelField($"Max values: {main.max_values * 2}   (x2)");
    }

    // Unity Menu item for creating a new PathTransform object
    [MenuItem("GameObject/Create PathTransform object", priority = 0)]
    public static void Create()
    {
        GameObject obj = new GameObject() { name = "PathTransform object" };
        obj.AddComponent<PathTransform>();
    }
}