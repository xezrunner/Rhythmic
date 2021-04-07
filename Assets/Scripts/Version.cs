using System;

public static class Version
{
    /// <summary>Returns the build string in YYYYMMDD-HHMM format.</summary>
    public static string build_string
    {
        get
        {
            return $"Build {DateTime.Now.Date.Year}{DateTime.Now.Month.ToString("00")}{DateTime.Now.Day.ToString("00")}" +
                           $"-{DateTime.Now.Hour.ToString("00")}{DateTime.Now.Minute.ToString("00")}";
        }
    }
}