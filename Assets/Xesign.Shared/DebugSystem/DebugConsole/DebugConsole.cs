using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DebugCom("Prefabs/DebugSystem/DebugConsole")]
public partial class DebugConsole : DebugCom
{
    public static DebugConsole Instance;
    DebugSystem DebugSystem;

    public override void Awake()
    {
        base.Awake();
        Instance = this;
        DebugSystem = DebugSystem.Instance;
    }

    public void Update()
    {
        UPDATE_Input();
        UPDATE_Openness();
        UPDATE_Scroll();
    }
}
