using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerPowerupManager))]
public class PowerupManagerEditor : Editor
{
    PlayerPowerupManager main;
    void Awake() => main = (PlayerPowerupManager)target;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!Application.isPlaying)
        {
            EditorGUILayout.LabelField("Game session inactive.");
            return;
        }

        foreach (Powerup p in main.Powerups)
        {
            if (p && GUILayout.Button($"Deploy {p.Name}"))
                p.Deploy();
        }
    }
}
