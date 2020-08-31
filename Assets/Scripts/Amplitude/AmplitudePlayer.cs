using System;
using System.Collections;
using UnityEngine;

public class AmplitudePlayer : Player
{
    public AmplitudeSongController amp_ctrl;

    public override void Start()
    {
        base.Start();

        // Wire up catcher events
        CatcherController.OnCatch += CatcherController_OnCatch;
    }

    void CatcherController_OnCatch(object sender, CatcherController.CatchEventArgs e)
    {

    }

    // MAIN LOOP

    public override void Update()
    {
        // base update
        base.Update();

        if (RhythmicGame.IsLoading)
            return;

        // PLAYER MOVEMENT
        if (IsSongPlaying)
        {
            // get z position
            //float zPos = amp_ctrl.songPosition * amp_ctrl.secPerBeat * PlayerSpeed * amp_ctrl.TunnelSpeedAccountation - StartZOffset - ZOffset;

            // apply z position, taking into account the offset props
            //transform.position = new Vector3(transform.position.x, transform.position.y, zPos);

            // ***** NEW smoothened, interpolated camera movement ***** //
            // TODO: improve code readibility!
            float step = (PlayerSpeed * amp_ctrl.secPerBeat / amp_ctrl.TunnelSpeedAccountation) * Time.unscaledDeltaTime * amp_ctrl.songSpeed;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, transform.position.y, TracksController.Tracks[0].transform.lossyScale.z), step);

            // TODO: re-add player movement debug
                //if (RhythmicGame.DebugPlayerMovementEvents)
        }

        if (Input.GetKeyDown(KeyCode.Return))
            BeginPlay();
    }

    public override void BeginPlay()
    {
        // Start music in AMP song controller
        amp_ctrl.PlayMusic();
        // Start player movement
        IsSongPlaying = true;
    }
}