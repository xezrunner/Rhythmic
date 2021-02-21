using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[DebugComponent(DebugControllerState.DebugUI, typeof(DebugStats))]
public class DebugStats : DebugComponent
{
    public static DebugStats Instance;
    public static DebugComponentAttribute Attribute { get { return (DebugComponentAttribute)System.Attribute.GetCustomAttribute(typeof(DebugStats), typeof(DebugComponentAttribute)); } }

    DebugUI DebugUI { get { return DebugUI.Instance; } }
    SongController SongController { get { return SongController.Instance; } }
    TracksController TracksController { get { return TracksController.Instance; } }

    public bool IsEnabled = true;
    
    void Awake() => Instance = this;

    void Update()
    {
        string s;

        if (!SongController.IsEnabled)
        {
            s = "SongController Enabled: False";
            return;
        }

        int trackCount = TracksController.Instance.Tracks.Count;
        string trackNames = "";
        TracksController.Instance.Tracks.ForEach(t => trackNames += $"{t.TrackName}  ");

        s = $"World: DevScene\n" +
                   $"Room path: /rooms/_u_trans_/dev/dev_scene.drm [SceneToRoom]\n\n" +

                   $"SongController Enabled: {SongController.IsEnabled}\n" +
                   $"Song name: {SongController.songName}\n" +
                   $"Song BPM: {SongController.songBpm}  Song scale: {SongController.songFudgeFactor.ToString("0.00")}\n\n" +

                   $"Tracks: {trackNames}({trackCount})\n\n" +

                   $"SlopMs: {SongController.SlopMs}  SlopPos: {SongController.SlopPos}\n\n" +

                   $"Timscale: [world: {Time.timeScale.ToString("0.00")}]  [song: {SongController.songTimeScale.ToString("0.00")}]\n" +
                   $"Clock seconds: {Clock.Instance.seconds}\n" +
                   $"Clock bar: {(int)Clock.Instance.bar}\n" +
                   $"Clock beat: {(int)Clock.Instance.beat % 8} ({(int)Clock.Instance.beat})\n" +
                   $"Locomotion distance: {AmpPlayerLocomotion.Instance.DistanceTravelled}\n" +

                   //$"LightManager: null | LightGroups:  (0)";
                   "";

        DebugUI.Text = s;
    }
}