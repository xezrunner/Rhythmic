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

    void OnEnable()
    {
        pos = serializedObject.FindProperty("pos");
        rot = serializedObject.FindProperty("euler_rot");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(pos);
        EditorGUILayout.PropertyField(rot);

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }

    // Unity Menu item for creating a new PathTransform object
    [MenuItem("GameObject/Create PathTransform object", priority = 0)]
    public static void Create()
    {
        GameObject obj = new GameObject() { name = "PathTransform object" };
        obj.AddComponent<PathTransform>();
    }
}