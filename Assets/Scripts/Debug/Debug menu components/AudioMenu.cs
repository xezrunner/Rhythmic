namespace DebugMenus
{
    class AudioMenu : DebugMenuComponent
    {
        public static AudioMenu Instance;

        public override void Init()
        {
            base.Init();
            Instance = this;

            AddEntry("audio", false);
        }
    }
}
