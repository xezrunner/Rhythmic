using UnityEngine;

public class GameVariables : MonoBehaviour
{
    [Header("Common game variables")]
    /// <summary>Tunnel mode transforms the highway into a tunnel by rotating tracks around 360 degrees.</summary>
    public bool tunnel_mode = false;


    [Header("Track system")]
    /// <summary>The amount of milliseconds to wait between track instantiations.</summary>
    public float inst_delay_ms = 0;
}
