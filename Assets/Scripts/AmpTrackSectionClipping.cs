using UnityEngine;

/// <summary>
/// This class provides the horizon clipping for the measures and notes.
/// </summary>

public class AmpTrackSectionClipping : MonoBehaviour
{
    public AmpTrackSection Measure;

    float lastHorizonValue = 0f;

    public void LateUpdate()
    {
        float horizonValue = AmpPlayerLocomotion.Instance.HorizonLength - SongController.Instance.measureLengthInzPos - RhythmicGame.HorizonMeasuresOffset;

        if (horizonValue == lastHorizonValue) return;

        if (Measure.Position.z > horizonValue)
        {
            if (!Measure.IsCapturing & !Measure.IsCaptured)
                Measure.LengthClip();

            lastHorizonValue = horizonValue;
        }
        else
        {
            Measure.ClipManager.plane = null;
            Destroy(this);
        }
    }
}