/// This should be instantiated for every song, as it contains song-specific values.

public class SongTimeUnit
{
    public SongTimeUnit(float bpm)
    {
        song_bpm = bpm;
        CalculateTimeUnits();
    }
    
    public float song_bpm = 0f;
    
    // TODO: implement tunnel scaling    
    public float song_tunnel_scale = 1.0f;
    public float game_tunnel_scale = 1.0f; // Difficulty-based
    public float total_tunnel_scale = 1.0f; // TODO: Calculate total!
    
    public float beat_per_ms;
    public float ms_per_beat;
    public float sec_per_beat;
    
    // TODO: Not sure whether these should be constants, or whether they should be set
    // with initialization.
    public int beat_ticks = 480;
    public int bar_ticks = 1920;
    
    // TODO: Ticks should probably be of type 'long'.
    public float tick_in_ms;
    public float MsToTick(float ms) => tick_in_ms * ms;
    public float tick_in_sec;
    public float SecToTick(float sec) => tick_in_sec * sec;
    
    public float ms_in_tick;
    public float TickToMs(long ticks) => ms_in_tick * ticks;
    public float sec_in_tick;
    public float TickToSec(long ticks) => sec_in_tick * ticks;

    public float pos_in_sec;
    public float SecToPos(float sec) => pos_in_sec * sec;
    public float pos_in_tick; // How many posonds are there in a tick?
    public float TickToPos(long ticks) => pos_in_tick * ticks;
    public float pos_in_bar;
    public float BarToPos(int bar) => TickToPos(bar_ticks * bar);
    public float pos_in_ms;
    public float MsToPos(float ms) => pos_in_ms * ms;
    
    
    
    void CalculateTimeUnits() 
    {
        if (song_bpm == 0) return;
        
        beat_per_ms = (song_bpm / 60000f);
        ms_per_beat = (60000f / song_bpm);
        sec_per_beat = (60f / song_bpm);
        
        tick_in_ms = (song_bpm * beat_ticks) / 60000f;
        tick_in_sec = (song_bpm * beat_ticks) / 60f;
        
        ms_in_tick = 60000f / (song_bpm * beat_ticks);
        sec_in_tick = 60f / (song_bpm * beat_ticks);
        
        pos_in_tick = (4f / 480f) + ((4f / 480f) * song_tunnel_scale); // TODO: is this correct?
        pos_in_sec = (4f / sec_per_beat) + ((4f / sec_per_beat) * song_tunnel_scale);
        pos_in_ms = (4f / ms_per_beat) + ((4f / ms_per_beat) * song_tunnel_scale);
    }


}