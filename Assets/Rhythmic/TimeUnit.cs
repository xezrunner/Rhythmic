using static Logger;

public class TimeUnit
{
    public TimeUnit(Song song)
    {
        // TODO: revise this initialization!
        this.song_system = SongSystem.Instance; // TODO: temp?
        this.song = song;

        bpm = song.bpm;
        if (bpm == 0 && LogE("BPM can't be 0!".T(this))) return;

        Vars = GameState.Variables;
        beat_ticks = Vars.beat_ticks; // TOOD: investiagte different beat ticks for some AMP songs!
        tunnel_scale = song_system.tunnel_scale_global + song.tunnel_scale; // TODO: properly calculate total tunnel scale!
        Calculate();
    }

    SongSystem song_system;
    Song song;
    GameVariables Vars;

    public float bpm;
    public int beat_ticks;
    public float tunnel_scale = 1.0f;

    // ----- //

    public long tick_per_ms;
    public float ms_per_tick;
    public float beat_per_ms, ms_per_beat;
    public float pos_per_tick, tick_per_pos;
    public float pos_per_ms, ms_pres_pos;

    void Calculate()
    {
        beat_per_ms = (bpm / 60000f);
        ms_per_beat = (60000f / bpm);

        tick_per_ms = (long)(bpm * beat_ticks / 60000f); // TODO: is this OK?
        ms_per_tick = 60000f / (bpm * beat_ticks);

        // tunnel_scale has to be taken into account when calculating position:
        // original:
        // (4f / 480f) + ((4f / 480f) * song_tunnel_scale); // TODO: is this correct?
        pos_per_tick = (4f / beat_ticks) * 2 * tunnel_scale;
    }
}