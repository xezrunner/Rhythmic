using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSystem : MonoBehaviour
{
    public static WorldSystem Instance;
    public void Awake() => Instance = this;

    public string Name = "World_";

    public PathCreator pathcreator;

    public static PathCreator GetAPathCreator()
    {
        if (Instance && Instance.pathcreator) return Instance.pathcreator;
        return FindObjectOfType<PathCreator>();
    }
}