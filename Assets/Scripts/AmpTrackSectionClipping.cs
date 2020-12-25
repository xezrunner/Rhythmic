using UnityEngine;

public class AmpTrackSectionClipping : MonoBehaviour
{
    public AmpTrackSection Measure;

    public void LateUpdate()
    {
        if (Measure.Position.z > AmpPlayerLocomotion.Instance.HorizonLength - SongController.Instance.measureLengthInzPos - RhythmicGame.HorizonMeasuresOffset)

            Measure.LengthClip();
        else
        {
            Measure.ClipManager.plane = null;
            Destroy(this);
        }
    }
}