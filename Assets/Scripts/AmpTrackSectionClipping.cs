using UnityEngine;

/// <summary>
/// This class provides the horizon clipping for the measures and notes.
/// </summary>

public class AmpTrackSectionClipping : MonoBehaviour
{
    public AmpTrackSection Measure;

    public void LateUpdate()
    {
        if (Measure.Position.z > AmpPlayerLocomotion.Instance.HorizonLength - SongController.Instance.measureLengthInzPos - RhythmicGame.HorizonMeasuresOffset)
        {
            if (!Measure.IsCapturing & !Measure.IsCaptured)
                Measure.LengthClip();
        }
        else
        {
            Measure.ClipManager.plane = null;
            Destroy(this);
        }
    }
}