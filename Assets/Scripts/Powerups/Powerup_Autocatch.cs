[Powerup(PowerupType.Autocatch, "Autocatch", "cleanse_deploy", bar_length: -1)]
public class Powerup_Autocatch : Powerup
{
    TracksController TracksController { get { return TracksController.Instance; } }

    public override void Deploy()
    {
        base.Deploy();
        Logger.Log("AUTOCATCH!".AddColor(Colors.Network));

        TracksController.CaptureMeasureAmount(Clock.Fbar, RhythmicGame.TrackCaptureLength, TracksController.CurrentTrack);
    }
}