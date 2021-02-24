using System.Collections.Generic;
using UnityEngine;

public class LightGroup : MonoBehaviour
{
    public string Name;

    public List<WorldLight> Lights = new List<WorldLight>();
}

public class LightManager : MonoBehaviour
{
    public static LightManager Instance;

    public List<LightGroup> LightGroups = new List<LightGroup>();

    void Awake()
    {
        if (Instance) // Do not allow multiple instances!
        {
            Debug.LogError($"LightManager [init]: Only one instance of a LightSystem can exist! [current: {Instance.gameObject.name}]");
            return;
        }
        Instance = this;
    }

    void Start()
    {
        Debug.Log($"LightManager: Started [object: {gameObject.name}]");
    }

}