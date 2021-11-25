using UnityEngine;

public class PlayerInputDispatcher : MonoBehaviour {
    public Player player;
    public PlayerLocomotion locomotion;
    public PlayerTrackSwitching track_switching;

    public void OnSwitchTrackLeft() => track_switching.SwitchTrack(SwitchTrackDir.Left);
    public void OnSwitchTrackRight() => track_switching.SwitchTrack(SwitchTrackDir.Right);

}