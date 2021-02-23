[DebugComponent(DebugComponentFlag.DebugInterfaces, DebugComponentType.Component, 250, 12)]
public class SelectionComponentTest : DebugComponent
{
    public static RefDebugComInstance Instance;
    private void Awake() => Instance = new RefDebugComInstance(this);

    int counter = 1;
    public override void UI_Main()
    {
        AddLine($"{counter++}");
    }
}