using System;
using UnityEngine;

public enum NoteCaptureFX
{
    _CatcherEffect = 0,
    _DestructEffect = 1 << 0,
    DotLightEffect = 1 << 1,

    CatcherCapture = _CatcherEffect | _DestructEffect,
    DestructCapture = _DestructEffect | DotLightEffect
}

public class AmpNoteFX : MonoBehaviour
{
    public AmpNote Note;

    public NoteCaptureFX? Effect = null;
    public bool DestroyOnCompletion = true;
    public float Step { get { return SongController.Instance.step; } }

    float fraction;
    public void ResetFX() => fraction = 0;

    void Update()
    {
        if (!Effect.HasValue)
            return;

        NoteCaptureFX fx = Effect.Value;

        if (fx.HasFlag(NoteCaptureFX._CatcherEffect))
        {
        }
        if (fx.HasFlag(NoteCaptureFX._DestructEffect))
        {
        }
        if (fx.HasFlag(NoteCaptureFX.DotLightEffect))
        {
            float intensity = SongController.Instance.intensity * (1 - fraction);
            Note.DotLightGlowIntensity = Mathf.Clamp(intensity, 1, 100);
        }

        fraction += Step * Time.deltaTime;

        if (DestroyOnCompletion && fraction >= 1f)
            Destroy(this);
    }
}
