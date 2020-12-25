using UnityEngine;

public class AmpTrackSectionClipping : MonoBehaviour
{
    public AmpTrackSection Measure;

    public void LateUpdate()
    {
        if (Measure.Position.z > AmpPlayerLocomotion.Instance.DistanceTravelled + 
            (RhythmicGame.HorizonMeasures - 1) * SongController.Instance.measureLengthInzPos)

            Measure.LengthClip();
        else
            Destroy(this);
    }
}