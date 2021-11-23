using System.Linq;

[DebugCom(/*350f*/ 60f)]
public class TrackStreamerDebugCom : DebugCom
{
    AudioSystem audio_system = SongSystem.Instance.audio_system;
    TrackStreamer streamer = TrackStreamer.Instance;
    Clock clock = Clock.Instance;

    public override string Com_Main()
    {
        Com_Clear();

        Com_WriteLine("Clock seconds: %", clock.seconds);
        Com_WriteLine("Clock beat: %", clock.beat);
        Com_WriteLine("Clock bar: %", clock.bar);
        Com_WriteLine("Clock pos: %", clock.pos);

        Com_WriteLine("Audio prev_deltatime: %", audio_system.prev_audio_deltatime);
        Com_WriteLine("Audio deltatime: %\n", audio_system.audio_deltatime);

        Com_WriteLine("Recycled sections: %", streamer.recycled_sections.Count);
        Com_WriteLine("Recycled notes: %", streamer.recycled_notes.Count);

        // ----- //

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