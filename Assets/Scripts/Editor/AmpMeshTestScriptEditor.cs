using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (script.TestDivisibility_Editor & !script.TestPieceSizingDivisibility())
            GUILayout.Label("At least one piece sizing is not divisible!", ErrorTextStyle);

        GUILayout.Label("Testing tools");

        if (GUILayout.Button("Create object"))
            script.CreateGameObject();

        if (GUILayout.Button("Update last object"))
            script.UpdateGameObject();

        if (GUILayout.Button("Delete last object"))
            DestroyImmediate(script.lastGo);
    }
}
