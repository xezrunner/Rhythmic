using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE: Scene type is not yet supported!
public enum SkyboxType { Simple = 0, Image = 1, Scene = 2 }

public class Skybox : MonoBehaviour
{
    WorldSystem WorldSystem { get { return WorldSystem.Instance; } }

    [Header("Skybox info")]
    public string Name = "_skybox";
    public string FriendlyName = "A Skybox";

    [Header("Global objects")]
    public LightManager LightManager;
    public Camera Camera;

    public List<LightGroup> LightGroups = new List<LightGroup>();

    void OnDestroy()
    {
        // Remove our LightGroups from WorldSystem
        LightGroups.ForEach(i => WorldSystem.LightManager.LightGroups.Remove(i));
    }

    void Start()
    {
        if (!WorldSystem)
        { Logger.LogMethodW("WorldSystem is null!", this); return; }

        // Add LightGroups to WorldSystem
        WorldSystem.AddLightGroups(LightGroups);
    }
}
