using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmpPlayerInputHandler : MonoBehaviour
{
    public AmpPlayer Player;
    public AmpPlayerTrackSwitching TrackSwitching;
    SongController SongController { get { return SongController.Instance; } }

    void OnTrackSwitchLeft() => TrackSwitching.SwitchTowardsDirection(HDirection.Left);
    void OnTrackSwitchLeftForce() => TrackSwitching.SwitchTowardsDirection(HDirection.Left, true);
    void OnTrackSwitchRight() => TrackSwitching.SwitchTowardsDirection(HDirection.Right);
    void OnTrackSwitchRightForce() => TrackSwitching.SwitchTowardsDirection(HDirection.Right, true);

    void OnCatchLeft() { }
    void OnCatchCenter() { }
    void OnCatchRight() { }

    void OnPowerup() { }
    void OnChangeCamera() { }

    void OnPlayPause() => SongController.PlayPause();
}
