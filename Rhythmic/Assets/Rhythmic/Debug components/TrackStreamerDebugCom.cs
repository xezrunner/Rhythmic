[DebugCom(/*350f*/ 60f)]
public class TrackStreamerDebugCom : DebugCom {
    AudioSystem   audio_system;
    TrackSystem   track_system ;
    TrackStreamer streamer ;
    Clock         clock ;

    public override void Awake() {
        base.Awake();

        audio_system = SongSystem.Instance.audio_system;
        track_system = TrackSystem.Instance;
        streamer = TrackStreamer.Instance;
        clock = Clock.Instance;
    }

    public override string Com_Main() {
        Com_Clear();

        Com_WriteLine("Clock seconds: %", clock.seconds);
        Com_WriteLine("Clock beat: %", clock.beat);
        Com_WriteLine("Clock bar: %", clock.bar);
        Com_WriteLine("Clock pos: %", clock.pos);

        Com_WriteLine("Audio prev_deltatime: %", audio_system.prev_audio_deltatime);
        Com_WriteLine("Audio deltatime: %", audio_system.audio_deltatime);
        Com_WriteLine("");

        // ----- //

        if (track_system.next_sections != null)
            Com_WriteLine("Next section IDs: %", track_system.DEBUG_NextSectionIDsToString());
        if (track_system.next_notes != null) {
            string s = null;
            foreach (Note it in track_system.next_notes)
                s += '(' + it.section.id.ToString().AddColor(it.section.track.info.instrument.color) + '/' +
                    it.id.ToString() + ") ";
            Com_WriteLine("Next notes:       %", s);
        }
        Com_WriteLine("");

        // ----- //

        Com_WriteLine("Recycled sections: %", streamer.recycled_sections.Count);
        Com_WriteLine("Recycled notes: %", streamer.recycled_notes.Count);


        Com_WriteLine("\nStream queue: %", streamer.stream_queue.Count);

        int stream_notes_sum = 0;
        foreach (var a in streamer.stream_notes_queue)
            stream_notes_sum = a.Count;
        Com_WriteLine("Stream (notes) queue: %", stream_notes_sum);

        Com_WriteLine("Recycle queue: %", streamer.recycle_queue.Count);

        int recycle_notes_sum = 0;
        foreach (var a in streamer.recycle_notes_queue)
            recycle_notes_sum = a.Count;
        Com_WriteLine("Recycle (notes) queue: %", recycle_notes_sum);

        // ----- //

        return base.Com_Main();
    }
}