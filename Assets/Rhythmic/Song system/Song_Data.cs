using System.Collections.Generic;

public class Song_Data
{
    public int note_count;

    // @ Optimize: consider using arrays instead of lists?
    public List<SongMetaNote> meta_notes = new();
}