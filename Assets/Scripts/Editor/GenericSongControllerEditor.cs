using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GenericSongController))]
public class GenericSongControllerEditor : Editor
{
    public GenericSongController main;
    
    void Awake() => main = (GenericSongController)target;
    
    public override void OnInspectorGUI() 
    {
        base.OnInspectorGUI();
        
        if (!Application.isPlaying)
        {
            GUILayout.Label("Tools unavailable - game isn't running.");
            return;
        }

        if (GUILayout.Button("Load song_test"))
            main.LoadSong("song_test");
    }
}