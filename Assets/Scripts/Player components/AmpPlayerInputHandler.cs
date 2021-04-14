using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmpPlayerInputHandler : MonoBehaviour
{
    SongController SongController { get { return SongController.Instance; } }

    public static AmpPlayerInputHandler Instance;

    public AmpPlayer Player;
    public AmpPlayerTrackSwitching TrackSwitching;
    public AmpPlayerCatching Catching;

    // TODO: Can this end up being stuck in certain situations?
    static bool _isActive = true;
    public static bool IsActive
    {
        get
        {
            //if (Instance == null) Logger.LogMethodW("requested (get), but instance is null!", "AmpPlayerInputHandler");
            return _isActive;
        }
        set
        {
            if (Instance == null) Logger.LogMethodW($"requested (set = {value}), but instance is null!", "AmpPlayerInputHandler");
            _isActive = value;
            if (Instance) Instance.gameObject.SetActive(value);
        }
    }

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(_isActive);
    }

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
