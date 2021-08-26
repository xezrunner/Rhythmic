using System;
using UnityEngine;

public class DebugComAttribute : Attribute
{
    public DebugComAttribute() { }
    public DebugComAttribute(float update_freq) { this.update_freq = update_freq; }
    public DebugComAttribute(string prefab_path, float update_freq = -1)
    {
        this.prefab_path = prefab_path;
        this.update_freq = update_freq;
    }

    public bool is_prefab { get { return prefab_path != null; } }
    public string prefab_path = null;
    public float update_freq = -1;
}

public class DebugCom : MonoBehaviour
{
    public DebugComAttribute Attribute;
    public Transform Prefab_Parent;

    public void Awake()
    {
        Attribute = (DebugComAttribute)System.Attribute.GetCustomAttribute(GetType(), typeof(DebugComAttribute));
        if (Attribute == null) Attribute = new DebugComAttribute();
    }
    public void Start()
    {
        // In case we have specified a prefab, but no parent was assigned:
        if (Attribute.is_prefab && !Prefab_Parent)
        {
            DebugSystem.Instance.AddUIDebugPrefab(Attribute.prefab_path);
            Destroy(this);
        }
    }
    public void Destroy()
    {
        if (Attribute.is_prefab) Destroy(Prefab_Parent);
        else Destroy(this);
    }

    public string id; // TODO: Performance!
    public string text;

    public void Clear() => text = "";
    public void Write(string t, params object[] args) => text += t.Parse(args); // TODO: Performance?
    // public void WriteLn(string t) => Text += t + "\n";

    /// This function is called when the component is active and requested.
    /// It is also called on every update frequency tick.
    public virtual string Main() { return text; }
}