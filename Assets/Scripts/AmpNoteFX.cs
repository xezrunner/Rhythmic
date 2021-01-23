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
    FXProperties FXProps { get { return FXProperties.Instance; } }
    public AmpNote Note;

    public NoteCaptureFX? Effect = null;
    public bool DestroyOnCompletion = true;

    float fraction;
    public void ResetFX() => fraction = 0;

    void Update()
    {
        if (!Effect.HasValue)
            return;

        NoteCaptureFX fx = Effect.Value;

        if (fx.HasFlag(NoteCaptureFX._CatcherEffect))
        { // tba
        }
        if (fx.HasFlag(NoteCaptureFX._DestructEffect))
        { // tba
        }
        if (fx.HasFlag(NoteCaptureFX.DotLightEffect))
        {
            float glowIntensity = FXProps.Note_DotLightGlowIntensity * (1 - fraction);
            Note.DotLightGlowIntensity = Mathf.Clamp(glowIntensity, 1, 100);
        }

        fraction += FXProps.Note_DotLightAnimStep * Time.deltaTime;

        if (DestroyOnCompletion && fraction >= 1f)
            Destroy(this);
    }
}
