using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmpPlayerInputHandler : MonoBehaviour
{
    SongController SongController { get { return SongController.Instance; } }

    public AmpPlayer Player;
    public AmpPlayerTrackSwitching TrackSwitching;
    public AmpPlayerCatching Catching;

    void OnTrackSwitchLeft() => TrackSwitching.SwitchTowardsDirection(HDirection.Left);
    void OnTrackSwitchLeftForce() => TrackSwitching.SwitchTowardsDirection(HDirection.Left, true);
    void OnTrackSwitchRight() => TrackSwitching.SwitchTowardsDirection(HDirection.Right);
    void OnTrackSwitchRightForce() => TrackSwitching.SwitchTowardsDirection(HDirection.Right, true);

    void OnCatchLeft() => Catching.TriggerCatcher(0);
    void OnCatchCenter() => Catching.TriggerCatcher(1);
    void OnCatchRight() => Catching.TriggerCatcher(2);

    void OnPowerup() { }
    void OnChangeCamera() { }

    void OnPlayPause() => SongController.PlayPause();
}
