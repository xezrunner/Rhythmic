using UnityEngine;

// Track destruction FX

public class AmpTrackDestructFX : MonoBehaviour
{
    [Header("Base particles refs")]
    public GameObject BaseParticlesObject;
    public ParticleSystem BaseParticles;

    [Header("Shard particles refs")]
    // NOTE: these should always contain 2 elements!
    public GameObject[] ShardParticleObjects;
    public ParticleSystem[] ShardParticles;
    public ParticleSystemRenderer[] ShardParticleRenderers;

    [Header("Base particles refs")]
    public ParticleSystem SparkleParticles;
    public ParticleSystemRenderer SparkleParticlesRenderer;

    [Header("Track properties")]
    public Color TrackColor;

    void Start()
    {
        // Decrease X scale for BaseParticles shape to not overlap other tracks - 0.6 seems good
        var shape = BaseParticles.shape;
        shape.scale = new Vector3(BaseParticles.shape.scale.x - .6f, BaseParticles.shape.scale.y, BaseParticles.shape.scale.z);

        if (ShardParticleObjects.Length != 2 || ShardParticles.Length != 2)
            Logger.LogMethodW("There are less/more than 2 elements in shard particle arrays!", null);

        // TODO: Do we really need to set regular Color as well?
        SparkleParticlesRenderer.material.SetColor("_EmissionColor", TrackColor);
        SparkleParticlesRenderer.material.SetColor("_Color", TrackColor);

        // Setup shard particle positions
        for (int i = 0; i < 2; i++)
            ShardParticleObjects[i].transform.position = new Vector3(RhythmicGame.TrackWidth / 2 * (i != 1 ? -1 : 1), 0, 0);

        // Setup shard particles:
        for (int i = 0; i < 2; i++)
        {
            ShardParticleRenderers[i].material.SetColor("_EmissionColor", TrackColor);
            // TODO: Do we really need to set regular Color as well?
            ShardParticleRenderers[i].material.SetColor("_Color", TrackColor);
        }
    }

    public void Play()
    {
        BaseParticles.Play();
        SparkleParticles.Play();
        for (int i = 0; i < 2; i++)
            ShardParticles[i].Play();
    }

    public void Stop()
    {
        BaseParticles.Stop();
        SparkleParticles.Stop();
        for (int i = 0; i < 2; i++)
            ShardParticles[i].Stop();
    }
}