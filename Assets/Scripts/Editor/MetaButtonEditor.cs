using UnityEditor;

[CustomEditor(typeof(MetaButton))]
public class MetaButtonEditor : Editor
{
    MetaButton main;
    void Awake() => main = (MetaButton)target;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        return;

        EditorGUILayout.Separator();
        
        //EditorGUILayout.LabelField("Not sure whether this should be used!", EditorStyles.boldLabel);

        // Properties section:
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);
        
        // Text:    
        main.SetText(EditorGUILayout.TextField("Text", main.UI_Text.text));
        
        // Size:
        main.Width = EditorGUILayout.FloatField("Width", main.Width);
        main.Height = EditorGUILayout.FloatField("Height", main.Height);
    }
}