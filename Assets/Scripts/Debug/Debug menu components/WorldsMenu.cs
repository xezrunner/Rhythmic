namespace DebugMenus
{
    public class WorldsMenu : DebugMenuComponent
    {
        public static WorldsMenu Instance;

        DebugStats Stats { get { return (DebugStats)DebugStats.Instance.Component; } }
        DebugMenu DebugMenu { get { return (DebugMenu)DebugMenu.Instance; } }

        public override void Init()
        {
            base.Init();
            Instance = this;

            AddEntry("Dev worlds: ", false);
            {
                AddEntry("  - DevScene", () => LoadWorld("DevScene"));
                AddEntry("  - DevScene_Sandbox", () => LoadWorld("DevScene_Sandbox"));
                AddEntry("  - TestScene", () => LoadWorld("TestScene"), "Space-themed abstract background test (xezrunner)");
            }
            AddEntry("Skybox: ", false);
            {
                AddEntry("  - ColorLightsBackground", () => LoadWorld("Skybox/ColorLightsBackground"));
            }
            AddEntry("PathTestScene", () => LoadWorld("PathTestScene"));
            AddEntry("_Testing/TunnelTesting", () => LoadWorld("_Testing/TunnelTesting"));
            AddEntry("Example/ssms_example", () => LoadWorld("Example/ssms_example"));
        }

        // Functionality:

        void LoadWorld(string world)
        {
            DebugMenu.SetActive(false);
            RhythmicGame.StartWorld = world;
            RhythmicGame.Restart();
        }
    }
}