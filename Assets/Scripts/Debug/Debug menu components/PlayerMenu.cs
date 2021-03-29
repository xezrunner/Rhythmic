namespace DebugMenus
{
    class PlayerMenu : DebugMenuComponent
    {
        public static PlayerMenu Instance;

        public override void Init()
        {
            base.Init();
            Instance = this;

            AddEntry("Enable player input", new Ref(() => AmpPlayerInputHandler.IsActive, (v) => AmpPlayerInputHandler.IsActive = (bool)v));
        }

        // Functionality:
    }
}
