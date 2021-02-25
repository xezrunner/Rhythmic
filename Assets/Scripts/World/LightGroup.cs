using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightGroup : MonoBehaviour
{
    public string Name { get { return gameObject.name; } set { gameObject.name = value; } }
    [HideInInspector] public int ID;

    public List<WorldLight> Lights = new List<WorldLight>();

    public void Add(WorldLight light) => Lights.Add(light);

    public WorldLight Find(string name, int id = 0) => Lights.Where(i => i.Name == name && i.ID == id).FirstOrDefault();
    public WorldLight Find(string name) => Lights.Where(i => i.Name == name).FirstOrDefault();
    public WorldLight Find(int id) => Lights.Where(i => i.ID == id).FirstOrDefault();
}