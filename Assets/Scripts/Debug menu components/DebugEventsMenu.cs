namespace DebugMenus
{
    class DebugEventsMenu : DebugMenuComponent
    {
        public static DebugEventsMenu Instance;

        public override void Init()
        {
            base.Init();
            Instance = this;

            AddEntry("Debug player camera anim events", new Ref(() => RhythmicGame.DebugPlayerCameraAnimEvents, (v) => RhythmicGame.DebugPlayerCameraAnimEvents = (bool)v));
            AddEntry("Debug player track switch events", new Ref(() => RhythmicGame.DebugPlayerTrackSwitchEvents, (v) => RhythmicGame.DebugPlayerTrackSwitchEvents = (bool)v));
            AddEntry("Debug player track seeking events", new Ref(() => RhythmicGame.DebugPlayerTrackSeekEvents, (v) => RhythmicGame.DebugPlayerTrackSeekEvents = (bool)v));
            AddEntry();

            AddEntry("Enable note target info", new Ref(() => AmpNote.DEBUG_ShowTargetNoteIndicators, (v) => AmpNote.DEBUG_ShowTargetNoteIndicators = (bool)v));
            AddEntry("Visualize note slop on catchers", new Ref(() => AmpPlayerCatching.IsSlopVisualization, (v) => AmpPlayerCatching.IsSlopVisualization = (bool)v));
            AddEntry("Debug catcher slop events", new Ref(() => RhythmicGame.DebugCatcherSlopEvents, (v) => RhythmicGame.DebugCatcherSlopEvents = (bool)v));
            AddEntry();

            AddEntry("Debug catch result events", new Ref(() => RhythmicGame.DebugCatchResultEvents, (v) => RhythmicGame.DebugCatchResultEvents = (bool)v));
            AddEntry("Debug target note refresh events", new Ref(() => RhythmicGame.DebugTargetNoteRefreshEvents, (v) => RhythmicGame.DebugTargetNoteRefreshEvents = (bool)v));
            AddEntry("Debug sequences refresh events", new Ref(() => RhythmicGame.DebugSequenceRefreshEvents, (v) => RhythmicGame.DebugSequenceRefreshEvents = (bool)v));
        }
    }
}