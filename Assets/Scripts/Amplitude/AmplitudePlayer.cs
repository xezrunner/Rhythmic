using System;
using System.Collections;
using UnityEngine;

public class AmplitudePlayer : Player
{
    // MAIN LOOP

    public override void Update()
    {
        // base update
        base.Update();

        if (RhythmicGame.IsLoading)
            return;

        // PLAYER MOVEMENT
        if (IsPlaying)
        {
            // get z position
            //float zPos = amp_ctrl.songPosition * amp_ctrl.secPerBeat * PlayerSpeed * amp_ctrl.TunnelSpeedAccountation - StartZOffset - ZOffset;

            // apply z position, taking into account the offset props
            //transform.position = new Vector3(transform.position.x, transform.position.y, zPos);

            

            // TODO: re-add player movement debug
                //if (RhythmicGame.DebugPlayerMovementEvents)
        }

        if (Input.GetKeyDown(KeyCode.Return))
            BeginPlay();
    }

    public override void BeginPlay()
    {
        // Start music in AMP song controller
        SongController.Play();
    }
}