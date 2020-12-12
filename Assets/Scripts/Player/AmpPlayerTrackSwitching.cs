using System.Collections;
using UnityEngine;

public class AmpPlayerTrackSwitching : MonoBehaviour
{
    [Header("Common")]
    public AmpPlayer Player;
    public Tunnel Tunnel { get { return Tunnel.Instance; } }
    public AmpTrackController TracksController { get { return AmpTrackController.Instance; } }
    public AmpPlayerLocomotion Locomotion;

    public void SwitchToTrack(int ID)
    {

    }
}