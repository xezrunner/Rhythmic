[DebugComponent(DebugComponentFlag.DebugInterfaces, DebugComponentType.Component /*, 250, 12*/ )]
public class SelectionComponentTest : DebugComponent
{
    public static RefDebugComInstance Instance;
    private void Awake() => Instance = new RefDebugComInstance(this);

    public override void UI_Main()
    {
        AddLine($"Hi! I'm the {Name.AddColor(Colors.Application)} component. Glad to meet you!");
        AddLine($"I serve the purpose of testing {"selectable entries".AddColor(Colors.Network)} within these debug components.", 2);

        AddLine("For now, I do not have anything to show. Come back later when this feature is implemented! ;)");
    }
}