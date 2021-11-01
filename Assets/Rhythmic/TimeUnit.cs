public enum TimeUnitType
{
    Tick = 0, tc = 0, Seconds = 1, s = 1, Milliseconds = 2, ms = 2,
    Bar = 3, Beat = 4, Meters = 5, m = 5
}
public struct TimeUnit
{
    public TimeUnit(float value, TimeUnitType type)
    {
        this.value = value;
        this.type = type;
    }

    public float value;
    public TimeUnitType type;


}