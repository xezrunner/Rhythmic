using UnityEngine;

[Powerup(PowerupType.Autocatch, "Autocatch", "cleanse_deploy", bar_length: -1)]
public class Powerup_Autocatch : Powerup
{
    TracksController TracksController { get { return TracksController.Instance; } }
    PlayerLocomotion PlayerLocomotion { get { return PlayerLocomotion.Instance; } }

    [Header("Camera Shake")]
    public float shakeIntensity = 0.5f;
    public float shakeFrequency = 25f;

    Vector3 originalCameraPosition;

    public override void Deploy()
    {
        shaking = true;
        shakeT = 0f;
        originalCameraPosition = PlayerLocomotion.MainCamera.transform.localPosition;

        base.Deploy();
        Logger.Log("AUTOCATCH!".AddColor(Colors.Network));

        TracksController.CaptureMeasureAmount(Clock.Fbar, RhythmicGame.TrackCaptureLength, TracksController.CurrentTrack);
    }

    bool shaking = false;
    float shakeT = 1.1f;
    void Update()
    {
        if (!shaking) return;
        if (shakeT > 1f) shakeT = 1f;

        float intensity = shakeIntensity * (1f - shakeT);

        Vector3 shakeOffset = new Vector3(
            Mathf.Sin(Time.time * shakeFrequency) * intensity * Random.Range(-1f, 1f),
            Mathf.Sin(Time.time * shakeFrequency * 1.1f) * intensity * Random.Range(-1f, 1f),
            Mathf.Sin(Time.time * shakeFrequency * 0.9f) * intensity * Random.Range(-1f, 1f)
        );

        if (shakeT == 1f)
        {
            PlayerLocomotion.MainCamera.transform.localPosition = originalCameraPosition;
            shaking = false;
        }
        else
        {
            PlayerLocomotion.MainCamera.transform.localPosition = originalCameraPosition + shakeOffset;
            shakeT += Time.deltaTime;
        }
    }
}