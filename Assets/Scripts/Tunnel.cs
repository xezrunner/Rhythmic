using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// This class is used to hold information and position/angle data for the tunnel.
public class Tunnel : MonoBehaviour
{
    public static Tunnel Instance;

    public int trackCount; // How many tracks there are, including duplicated ones

    public float rotZ; // one track's angle

    public float outline; // size of the tunnel's circle outline
    /// <summary>
    /// Half width of the circle (from center)
    /// </summary>
    public float radius;
    /// <summary>
    /// Full width of the circle (from 0, center)
    /// </summary>
    public float diameter;

    public Vector3 center; // the center points of the circle

    void Awake() { Instance = this; }

    public void Init(int count)
    {
        if (count < 2)
        { Debug.LogErrorFormat("TUNNEL: can't create tunnel from less than 2 tracks! Track count: {0}", trackCount); return; }

        trackCount = count;

        rotZ = 360 / trackCount;

        outline = trackCount * RhythmicGame.TrackWidth;
        radius = -outline / (2f * Mathf.PI) + 0.205f;
        diameter = radius * 2f;

        center = new Vector2(0, -radius);
    }

    public Vector3[] GetTransformForTrackID(int id)
    {
        Vector3[] transform = new Vector3[2];

        if (!RhythmicGame.IsTunnelMode)
            transform[0] = new Vector3(id * RhythmicGame.TrackWidth, 0, 0);
        else
        {
            float angle = id * -rotZ;
            float posX = radius * Mathf.Sin(angle * Mathf.Deg2Rad) + center.x;
            float posY = radius * Mathf.Cos(angle * Mathf.Deg2Rad) + center.y;

            transform[0] = new Vector3(posX, posY);
            transform[1].z = -angle;
        }

        return transform;
    }
}