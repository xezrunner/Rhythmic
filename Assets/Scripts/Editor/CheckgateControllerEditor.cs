using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CheckgateController))]
public class CheckgateControllerEditor : Editor
{
    public CheckgateController main;

    void Awake() => main = (CheckgateController)target;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        GUILayout.Label("anim_index: " + main.anim_index);
        GUILayout.Label("anim_value: " + main.anim_value);
        GUILayout.Label("anim_index_time: " + main.anim_index_time);
        
        if (GUILayout.Button("Trigger action!"))
            main.Checkgate_Action();
    }
}