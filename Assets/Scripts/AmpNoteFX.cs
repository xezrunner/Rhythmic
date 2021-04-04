using System;
using UnityEngine;

[Flags]
public enum NoteCaptureFX
{
    None = 0,
    _CatcherEffect = 1 << 0,
    _DestructEffect = 1 << 1,
    DotLightEffect = 1 << 2,

    CatcherCapture = _DestructEffect | _CatcherEffect,
    DestructCapture = DotLightEffect | _DestructEffect
}

public class AmpNoteFX : MonoBehaviour
{
    FXProperties FXProps { get { return FXProperties.Instance; } }
    public AmpNote Note;

    [Header("References to contents")]
    public Animator BlastFX_Animator;

    public NoteCaptureFX? Effect = null;

    [Header("Properties")]
    public bool IsPlaying = false;
    public bool DestroyOnCompletion = true;

    float fraction;
    public void ResetFX() => fraction = 0;

    public void Play(NoteCaptureFX? _fx = null)
    {
        if (!_fx.HasValue) { Effect = null; return; }

        NoteCaptureFX fx = _fx.Value;
        Effect = fx;

        ResetFX();
        IsPlaying = true;

        if (fx.HasFlag(NoteCaptureFX._CatcherEffect))
            if (BlastFX_Animator) BlastFX_Animator.gameObject.SetActive(true); // TODO: temp! This keeps on playing, don't know how to reset it etc...
    }

    void Update()
    {
        if (!IsPlaying) return;
        if (!Effect.HasValue)
            return;

        NoteCaptureFX fx = Effect.Value;

        if (fx.HasFlag(NoteCaptureFX._CatcherEffect))
        {
        }
        if (fx.HasFlag(NoteCaptureFX._DestructEffect))
        { // tba
        }
        if (fx.HasFlag(NoteCaptureFX.DotLightEffect))
        {
            float glowIntensity = FXProps.Note_DotLightGlowIntensity * (1 - fraction);
            Note.DotLightGlowIntensity = Mathf.Clamp(glowIntensity, 1, 100);
        }

        if (fraction >= 1f)
        {
            if (DestroyOnCompletion) Destroy(this);
            IsPlaying = false;
            fraction = 0;
            return;
        }

        fraction += FXProps.Note_DotLightAnimStep * Time.deltaTime;
    }
}