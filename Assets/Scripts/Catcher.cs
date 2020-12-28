using System.Collections.Generic;
using UnityEngine;

public enum CatcherSide { Left = 0, Center = 1, Right = 2 }

public class Catcher : MonoBehaviour
{
    SongController SongController { get { return SongController.Instance; } }
    TracksController TracksController { get { return TracksController.Instance; } }

    [Header("Automatically assigned from Catching")]
    public AmpPlayer Player;
    public AmpPlayerLocomotion Locomotion;
    public AmpPlayerCatching Catching;

    [Header("Properties")]
    public int ID;
    public string Name
    {
        get
        {
            if (RhythmicGame.GameMatchTpye == GameMatchType.Singleplayer)
                return Side.ToString();
            else
                return $"{Player.Name}_{Side.ToString()}"; // Return the player name + catcher side ["Player1_Center"]
        }
    }

    public CatcherSide Side = CatcherSide.Center;

    // ... //
    // Visuals
    // Animations
    // Effects
    // ... //

    CatchResult GenerateFail(float dist)
    {
        // Evaluate whether we are under a measure and return the appropriate fail catch result
        // for the scenario.

        AmpTrackSection measure = TracksController.CurrentTrack.CurrentMeasure;

        if (measure.IsEmpty || measure.IsCaptured)
            return new CatchResult(this, CatchResultType.Empty, null);

        float slopMs = RhythmicGame.SlopMs;

        float mStart = measure.Position.z;
        float mLength = measure.Length;
        float mFrac = (mStart - dist) / mLength; // Fraction (0-1) player distance in the measure

        int noteNum = (int)mFrac * measure.Notes.Count;
        AmpNote note = measure.Notes[noteNum];

        if (note.Distance > dist + slopMs || note.Distance < dist - slopMs)
            return new CatchResult(this, CatchResultType.Miss, null); // Missed with no particular note
        else
            return new CatchResult(this, CatchResultType.Miss, note); // Missed with a note
    }

    public CatchResult Catch()
    {
        float dist = Locomotion.DistanceTravelled; // Distance travelled
        float speed = Locomotion.Speed; // How many units do we traverse in a second?

        AmpNote target = Catching.targetNotes[TracksController.CurrentTrackID];

        // There is a target note to catch!
        if (target)
        {
            float targetDist = target.Distance;

            float slopMs = RhythmicGame.SlopMs;

            // Evaluate where we hit
            float diff = Mathf.Abs(dist - targetDist); // meters
            float diffSec = diff / speed; // seconds
            float diffMs = diffSec * 1000; // milliseconds

            // If the difference between target note and current distance is less than slopMs, we're good!
            if (diffMs < slopMs)
                return new CatchResult(this, CatchResultType.Success, target);
            else // Otherwise, find what note is closest
                return GenerateFail(dist);
        }
        else
        {
            Debug.LogError($"Catcher: there was no target note! | track: {TracksController.CurrentTrackID}, dist: {Locomotion.DistanceTravelled}");
            Catching.RefreshTargetNotes();
        }

        // ----

        return new CatchResult();
    }
}