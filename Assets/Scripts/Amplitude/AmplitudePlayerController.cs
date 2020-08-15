using System;
using System.Collections;
using UnityEngine;

public class AmplitudePlayerController : PlayerController
{
    AmplitudeSongController amp_ctrl { get { return AmplitudeSongController.Instance; } }

    new void Start()
    {
        base.Start();

        // Wire up catcher events
        CatcherController.OnCatch += CatcherController_OnCatch;
    }

    void CatcherController_OnCatch(object sender, CatcherController.CatchEventArgs e)
    {
        
    }

    // MAIN LOOP

    new void Update()
    {
        // base update
        base.Update();

        // PLAYER MOVEMENT
        if (IsPlayerMoving)
        {
            // get z position
            float zPos = amp_ctrl.songPosition * amp_ctrl.secPerBeat * PlayerSpeed * amp_ctrl.TunnelSpeedAccountation - StartZOffset - ZOffset;

            // apply z position, taking into account the offset props
            transform.position = new Vector3(transform.position.x, transform.position.y, zPos);

            if (RhythmicGame.DebugPlayerMovementEvents)
                Debug.LogFormat("PLAYER: [DEBUG] songPosition: {0} | songPositionInBeats: {1}", amp_ctrl.songPosition, amp_ctrl.songPositionInBeats);
        }

        // ENTER Key
        if (Input.GetKeyDown(KeyCode.Return) & !RhythmicGame.IsLoading)
            BeginPlay();
    }

    public override void BeginPlay()
    {
        // Start music in AMP song controller
        amp_ctrl.PlayMusic();
        // Start player movement
        IsPlayerMoving = true;
    }
}