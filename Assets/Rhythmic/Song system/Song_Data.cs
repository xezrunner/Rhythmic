using System.Collections.Generic;

public class Song_Data
{
    // TODO: Perhaps this could be an array too?
    // We *might* want to remove entries from here for policy.
    public List<string> track_defs = new List<string>()
    {
        "drums0",
        "bass",
        "synth0",
        "synth1",
        "vocals",
        "drums1"
    };

    public int note_count;

    // @ Optimize: consider using arrays instead of lists?
    public List<SongMetaNote> meta_notes = new();
}