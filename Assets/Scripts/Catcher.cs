#undef VISUALIZE_SLOP

using UnityEngine;

public enum CatcherSide { Left = 0, Center = 1, Right = 2 }

public class Catcher : MonoBehaviour
{
    GenericSongController SongController { get { return GenericSongController.Instance; } }
    TracksController TracksController { get { return TracksController.Instance; } }
    GameState GameState { get { return GameState.Instance; } }

    [Header("Automatically assigned from Catching")]
    public Player Player;
    public PlayerLocomotion Locomotion;
    public PlayerCatching Catching;

    [Header("Properties")]
    public int ID;
    public string Name
    {
        get
        {
            if (GameState.GameMatchTpye == GameMatchType.Singleplayer)
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

        Measure measure = TracksController.CurrentTrack.CurrentMeasure;

        if (measure.IsEmpty || measure.IsCaptured)
            return new CatchResult(this, CatchResultType.Empty, null);

        float slopMs = RhythmicGame.SlopMs;
        float slopzPos = SongController.time_units.SecToPos(slopMs / 1000f);
        
        float mStart = measure.Position.z;
        float mLength = measure.Length;
        float mFrac = (mStart - dist) / mLength; // Fraction (0-1) player distance in the measure

        int noteNum = (int)mFrac * measure.Notes.Count;
        Note note = measure.Notes[noteNum];

        if (note.Distance > dist + slopzPos || note.Distance < dist - slopzPos)
            return new CatchResult(this, CatchResultType.Miss, null); // Missed with no particular note
        else
            return new CatchResult(this, CatchResultType.Miss, note); // Missed with a note
    }

    public CatchResult Catch()
    {
        // Handle empty catch
        Measure currentMeasure = TracksController.CurrentTrack.CurrentMeasure;
        if (!currentMeasure) return new CatchResult();
        if (currentMeasure.IsEmpty || currentMeasure.IsCaptured)
            return new CatchResult(this, CatchResultType.Empty, null);

        float dist = Locomotion.DistanceTravelled; // Distance travelled
        float speed = Locomotion.Speed; // How many units do we traverse in a second?

        Note target = TracksController.targetNotes[TracksController.CurrentTrackID];

        // There is a target note to catch!
        if (target)
        {
            float targetDist = target.Distance;

            float slopPos = SongController.time_units.MsToPos(RhythmicGame.SlopMs) / 2;

            // Evaluate where we hit
            float diff = Mathf.Abs(dist - targetDist); // meters
            //float diffSec = SongController.PosToSec(diff); // seconds
            //float diffMs = SongController.PosToMs(diff); // milliseconds

            //Debug.Log($"diff: {diff} | diffSec: {diffSec} | diffMs: {diffMs} | slopMs: {slopMs} :: speed: {speed}");

#if VISUALIZE_SLOP
            // DEBUG DRAW SLOP
            {
                GameObject debugBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
                debugBox.transform.parent = target.transform;
                debugBox.transform.position = target.transform.position;
                debugBox.transform.localScale = new Vector3(0.5f, 0.1f, SongController.SlopPos * 2);
                debugBox.transform.rotation = target.transform.rotation;
                debugBox.GetComponent<MeshRenderer>().material.color = Color.red;

                //Debug.Break();
            }
#endif

            // If the difference between target note and current distance is less than slopMs, we're good!
            if (diff < slopPos && (int)target.Lane == (int)Side)
                return new CatchResult(this, CatchResultType.Success, target);
            else // Otherwise, find what note is closest
                return GenerateFail(dist);
        }
        else
        {
            Debug.LogError($"Catcher: there was no target note! | track: {TracksController.CurrentTrackID}, dist: {Locomotion.DistanceTravelled}");
            TracksController.RefreshTargetNotes();
        }

        // ----

        return new CatchResult();
    }
}