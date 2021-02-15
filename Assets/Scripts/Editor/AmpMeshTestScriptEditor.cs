using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrackMeshCreator))]
class AmpMeshTestScriptEditor : Editor
{
    TrackMeshCreator script;
    void Awake()
    {
        script = (TrackMeshCreator)target;
        ErrorTextStyle.normal.textColor = Color.red;
    }

    GUIStyle ErrorTextStyle = new GUIStyle();
    bool debug = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (script.TestDivisibility_Editor & !script.TestPieceSizingDivisibility())
            GUILayout.Label("At least one piece sizing is not divisible!", ErrorTextStyle);

        GUILayout.Label("Testing tools");

        debug = GUILayout.Toggle(debug, "Debug draw vertices");

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Create object"))
            script.CreateGameObject(debug);

        if (GUILayout.Button("Update last object"))
            script.UpdateGameObject();

        GUILayout.EndHorizontal();

        if (GUILayout.Button("Delete last object"))
        {
            DestroyImmediate(script.lastGo);

            GameObject[] removeList = GameObject.FindGameObjectsWithTag("Remove");
            if (removeList.Length < 1) return;
            foreach (GameObject go in removeList)
                DestroyImmediate(go);
        }
    }
}
