using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AmpTrackController))]
public class AmpTrackControllerEditor : Editor
{
    AmpTrackController script;
    void Awake() => script = (AmpTrackController)target;

    GameObject trackSectionPrefab;
    AmpTrackSection lastSection;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Create test section"))
        {
            if (!trackSectionPrefab)
                trackSectionPrefab = (GameObject)Resources.Load("Prefabs/AmpTrackSection");

            if (!trackSectionPrefab)
                Debug.LogError("AmpTrackController: cannot load track section prefab!");
            else
            {
                GameObject go = Instantiate(trackSectionPrefab);
                AmpTrackSection s = go.GetComponent<AmpTrackSection>();
                s.PositionOnPath = script.TestTrackSectionPos;
                s.RotationOnPath = script.TestTrackSectionRot;

                lastSection = s;
            }
        }

    }
}
