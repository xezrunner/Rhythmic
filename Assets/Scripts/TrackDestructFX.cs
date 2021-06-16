using System.Collections;
using UnityEngine;

// Track destruction FX

public class TrackDestructFX : MonoBehaviour
{
    static FXProperties FXProps;

    public Light Light;
    public GameObject LightObject;

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

    [Header("Common")]
    public bool IsPlaying;
    public DestructFXPolicy Policy;

    private void Awake()
    {
        FXProps = FXProperties.Instance;
        if (FXProps == null)
        {
            FXProps = Player.Instance.gameObject.AddComponent<FXProperties>();
            Logger.LogW("There was no global FXProperties component - added one to the player.".T(this));
        }
        Policy = FXProps.Destruct_Policy;
    }
    void Start()
    {
        // Decrease X scale for BaseParticles shape to not overlap other tracks - 0.6 seems good
        var shape = BaseParticles.shape;
        shape.scale = new Vector3(BaseParticles.shape.scale.x - .6f, BaseParticles.shape.scale.y, BaseParticles.shape.scale.z);

        SetupColors();

        // Shard particles setup:
        if (ShardParticleObjects.Length != 2 || ShardParticles.Length != 2)
            Logger.LogMethodW("There are less/more than 2 elements in shard particle arrays!", null);
        // Position the 2 shards to horizontal edges of track
        for (int i = 0; i < 2; i++)
        {
            ShardParticleObjects[i].transform.position = new Vector3(RhythmicGame.TrackWidth / 2 * (i != 1 ? -1 : 1), 0, 0);
        }
    }

    public void SetupColors()
    {
        Light.color = TrackColor;

        // TODO: Do we really need to set regular _Color properties as well?
        // Sparkle particles
        SparkleParticlesRenderer.material.SetColor("_EmissionColor", TrackColor * FXProps.Destruct_SparkleGlow);
        SparkleParticlesRenderer.material.SetColor("_Color", TrackColor);

        // Shards:
        for (int i = 0; i < 2; i++)
        {
            ShardParticleRenderers[i].material.SetColor("_EmissionColor", TrackColor * FXProps.Destruct_ShardGlow);
            ShardParticleRenderers[i].material.SetColor("_Color", TrackColor);
        }
    }

    IEnumerator AnimateLight(float from, float to)
    {
        float t = 0;
        while (Light.intensity != to)
        {
            Light.intensity = Mathf.Lerp(from, to, t);
            t += Time.deltaTime;
            yield return null;
        }
    }

    // TODO: set certain properties based on proximity, such as how many particles to draw in certain particles?
    public void Play(bool proximity = true)
    {
        if (!proximity || IsPlaying) return;
        
        IsPlaying = true;
        
        //StartCoroutine(AnimateLight(0, 10));
        Light.intensity = 7;
        
        SparkleParticles.Play();
        BaseParticles.Play();
        for (int i = 0; i < 2; i++)
            ShardParticles[i].Play();
    }
    
    public void Stop()
    {
        //StartCoroutine(AnimateLight(10, 0));
        Light.intensity = 0;
        
        BaseParticles.Stop();
        SparkleParticles.Stop();
        for (int i = 0; i < 2; i++)
            ShardParticles[i].Stop();
        
        IsPlaying = false;
    }
}