namespace DebugMenus
{
    class PlayerMenu : DebugMenuComponent
    {
        public static PlayerMenu Instance;

        public override void Init()
        {
            base.Init();
            Instance = this;

            AddEntry("Check back later!", false);
        }

        // Functionality:
    }
}
