using System;
using UnityEngine;

[Flags]
public enum NoteCaptureFX
{
    None = 0,
    _CatcherEffect = 1 << 0,
    _DestructEffect = 1 << 1,
    DotLightEffect = 1 << 2,

    CatcherCapture = _CatcherEffect /*| DotLightEffect*/,
    DestructCapture = _DestructEffect | DotLightEffect
}

public class AmpNoteFX : MonoBehaviour
{
    FXProperties FXProps { get { return FXProperties.Instance; } }
    public AmpNote Note;

    [Header("References to contents")]
    public Animation BlastFX_Animator;
    public ParticleSystem CatcherHit_Particles;
    public ParticleSystem DestructHit_Particles;

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
        {
            if (BlastFX_Animator)
            {
                BlastFX_Animator.gameObject.SetActive(true); // TODO: temp! This keeps on playing, don't know how to reset it etc...
                BlastFX_Animator.Play();
            }
            if (CatcherHit_Particles)
            {
                //CatcherHit_Particles.gameObject.SetActive(true);
                //CatcherHit_Particles.Play();
            }
        }
        if (fx.HasFlag(NoteCaptureFX._DestructEffect))
            if (DestructHit_Particles)
            {
                DestructHit_Particles.gameObject.SetActive(true);
                DestructHit_Particles.Play();
            }
    }

    void Update()
    {
        if (!IsPlaying) return;
        if (!Effect.HasValue)
            return;

        NoteCaptureFX fx = Effect.Value;

        if (fx.HasFlag(NoteCaptureFX._CatcherEffect))
        {
            // TODO: TEMP: have blast move with catcher! (hacky)
            float offset = (Note.Lane == LaneSide.Center) ? 0f : (Note.Lane == LaneSide.Left ? -1.18f : 1.18f);
            Vector3 normal = (WorldSystem.Instance.Path != null) ? (WorldSystem.Instance.Path.GetNormalAtDistance(Mathf.Clamp(AmpPlayerLocomotion.Instance.DistanceTravelled, -10000, WorldSystem.Instance.Path.length - 0.001f))) : Vector3.right;
            //CatcherHit_Particles.transform.position = AmpPlayer.Instance.transform.position + (normal * offset);
            BlastFX_Animator.gameObject.transform.position = AmpPlayerCatching.Instance.CatcherVisuals.transform.position + (normal * offset);
            BlastFX_Animator.gameObject.transform.rotation = AmpPlayerCatching.Instance.CatcherVisuals.transform.rotation;

            if (fraction > 1f)
            {
                BlastFX_Animator.gameObject.SetActive(false);
                //CatcherHit_Particles.Stop();
                //CatcherHit_Particles.gameObject.SetActive(false);
            }
        }
        if (fx.HasFlag(NoteCaptureFX._DestructEffect))
        {
            if (fraction > 1f)
                DestructHit_Particles.gameObject.SetActive(false);
        }
        if (fx.HasFlag(NoteCaptureFX.DotLightEffect))
        {
            float glowIntensity = FXProps.Note_DotLightGlowIntensity * (1 - fraction);
            Note.DotLightGlowIntensity = Mathf.Clamp(glowIntensity, 1, 100);
        }

        if (fraction > 1f)
        {
            // Restore shared material to reduce constant draw calls due to different meshes
            Note.DotLightMeshRenderer.material = Note.Track.NoteDotLightMaterial;

            IsPlaying = false;
            fraction = 0;
            if (DestroyOnCompletion) Destroy(this);
            return;
        }

        fraction += FXProps.Note_DotLightAnimStep * Time.deltaTime;
    }
}