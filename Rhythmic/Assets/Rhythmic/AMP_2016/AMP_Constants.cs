public enum AMP_Instrument
{
    UNKNOWN = -1,
    Drums = 0, D = 0,
    Bass = 1, B = 1,
    Synth = 2, S = 2,
    Guitar = 3, G = 3,
    FX = 4,
    Vocals = 5, V = 5
}

/// NOTE: We use DIFFICULTY_NOTE_LANES for note lanes, as it makes it easier to deduct which lane 
/// it is based on its index (0-2).
public enum AMP_NoteLane
{
    BEGINNER_LEFT = 96, BEGINNER_CENTER = 98, BEGINNER_RIGHT = 100,
    INTERMEDIATE_LEFT = 102, INTERMEDIATE_CENTER = 104, INTERMEDIATE_RIGHT = 106,
    ADVANCED_LEFT = 108, ADVANCED_CENTER = 110, ADVANCED_RIGHT = 112,
    EXPERT_LEFT = 114, EXPERT_CENTER = 116, EXPERT_RIGHT = 118
}

public static class AMP_Constants
{
    public static string MOGGSONG_PATH = @"G:\amp_ps3\songs";
    public static string MIDI_PATH = MOGGSONG_PATH;
    public static string AUDIO_PATH = @"H:\HMXAMPLITUDE\ps4_songs";

    public static int[][] DIFFICULTY_NOTE_LANES =
    {
        new int[] { 96,98,100 },
        new int[] { 102,104,106 },
        new int[] { 108,110,112 },
        new int[] { 114,116,118 }
    };
    public static int GetNoteLaneIndexFromCode(int code)
    {
        for (int i = 0; i < DIFFICULTY_NOTE_LANES.Length; ++i)
        {
            for (int x = 0; x < DIFFICULTY_NOTE_LANES[i].Length; ++x)
            {
                int c = DIFFICULTY_NOTE_LANES[i][x];
                if (c == code) return x;
            }
        }

        return -1;
    }
    
}