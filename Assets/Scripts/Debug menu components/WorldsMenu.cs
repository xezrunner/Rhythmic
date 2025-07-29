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
                AddEntry("  - DevScene", () => LoadWorld(        "DevScene"));
                AddEntry("  - DevScene_Demo", () => LoadWorld(   "DevScene_Demo"));
                AddEntry("  - DevScene_Sandbox", () => LoadWorld("DevScene_Sandbox"));
                AddEntry("  - TestScene", () => LoadWorld("TestScene"), "Space-themed abstract background test (xezrunner)");
            }
            AddEntry("Skybox: ", false);
            {
                AddEntry("  - ColorLightsBackground", () => LoadWorld("ColorLightsBackground"));
            }

            AddEntry("Miscellaneous: ", false);
            
            AddEntry("Loading", () => LoadWorld("Loading"));
            AddEntry("PathTestScene", () => LoadWorld("PathTestScene"));
            AddEntry("_Testing/TunnelTesting", () => LoadWorld("TunnelTesting"));
            AddEntry("Example/ssms_example", () => LoadWorld("ssms_example"));
            
            AddEntry();

            AddEntry("MetaSystem/MetaSystem", () => LoadWorld("Meta/MetaSystem"));
            AddEntry("RH_Main", () => LoadWorld("RH_Main"), "New universe");
        }

        // Functionality:

        public static void LoadWorld(string world)
        {
            DebugMenu.SetActive(false);
            GameState.UnloadScene(world); // TEMP:
            GameState.LoadScene(world, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            //RhythmicGame.StartWorld = world;
            //RhythmicGame.Restart();
        }
    }
}