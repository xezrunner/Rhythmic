using System.Collections;
using UnityEngine;

[Powerup(PowerupType.Freestyle, "Freestyle", "flow_deploy", prefab_path: "Prefabs/Powerups/FreestylePowerup", auto_destroy: false)]
public class Powerup_Freestyle : Powerup
{
    PlayerLocomotion PlayerLocomotion { get { return PlayerLocomotion.Instance; } }
    public ParticleSystem Particles;

    public override void Deploy()
    {
        base.Deploy();
        Logger.Log("FREESTYLE!");
        
        PlayerLocomotion.IsFreestyle = true;
        Particles.Play();
    }

    public override void OnPowerupFinished()
    {
        base.OnPowerupFinished();
        Logger.LogW("FREESTYLE DONE - WAITING FOR PARTICLES");

        PlayerLocomotion.IsFreestyle = false;

        // Stop particles:
        Particles.Stop();

        // Destroy!
        StartCoroutine(Destroy_WaitForParticles());
    }

    IEnumerator Destroy_WaitForParticles()
    {
        while (Particles.IsAlive())
            yield return new WaitForSeconds(1);

        Logger.Log("FREESTYLE PARTICLES HAVE STOPPED EXISTING - DESTROYING");
        Destroy();
    }
}