// TODO: at the moment, this is basically useless.
// We'll want to do something interesting with version information storage & access (git branch info?).

public static class Version
{
    /// <summary>Returns the build string in YYYYMMDD-HHMM format.</summary>
    public static string build_string
    {
        get
        {
            /*
            return $"Build {DateTime.Now.Date.Year}{DateTime.Now.Month.ToString("00")}{DateTime.Now.Day.ToString("00")}" +
                           $"-{DateTime.Now.Hour.ToString("00")}{DateTime.Now.Minute.ToString("00")}";
            */
            return "2021-dev-edge";
        }
    }
}