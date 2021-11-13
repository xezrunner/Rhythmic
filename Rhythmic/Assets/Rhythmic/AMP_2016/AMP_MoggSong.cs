public struct AMP_MoggSong
{
    public string mogg_path;
    public string midi_path;

    public struct sect_song_info
    {
        public string length;
        public int countin;
    }
    public sect_song_info song_info;
}

public enum TimeUnit
{
    AbsoluteTicks = 0, absolute = 0, ticks = 0,
    Hours = 1, h = 1,
    Minutes = 2, m = 2,
    Seconds = 3, s = 3,
    Milliseconds = 4, ms = 4
}
public class TimeDef
{
    public TimeDef(float value, TimeUnit unit)
    {
        SetValue(value, unit);
    }

    public float _h;
    public float _m;
    public float _s;
    public float _ms;

    public TimeDef from_h(float h) => new TimeDef(h, TimeUnit.Hours);
    public TimeDef from_m(float m) => new TimeDef(m, TimeUnit.Minutes);
    public TimeDef from_s(float s) =>   new TimeDef(s, TimeUnit.Seconds);
    public TimeDef from_ms(float ms) => new TimeDef(ms, TimeUnit.Milliseconds);

    void SetValue(float value, TimeUnit unit)
    {

    }
}

public class test2
{
    void test()
    {
        AMP_MoggSong a = new AMP_MoggSong();
        
    }
}
