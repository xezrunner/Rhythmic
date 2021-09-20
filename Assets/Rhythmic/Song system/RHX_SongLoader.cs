using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Logger;

public static class RHX_SongLoader
{
    public const string RHX_FileExtension = ".rhx_song";

    public static Song LoadSong(string song_name)
    {
        Log("Loading song: '%'".T(nameof(RHX_SongLoader)), song_name);

        string song_path = FindSongFile(song_name);
        if (song_path == null && LogE("Could not find song: %".T(nameof(RHX_SongLoader)), song_name)) return null;

        string[] files = Directory.GetFiles(song_path);
        string rhx_song_file_path = null;

        // Find the .rhx_song file within the song directory:
        foreach (string f in files)
        {
            if (f.GetExt() != RHX_FileExtension) continue;
            rhx_song_file_path = f;
            break;
        }

        Song song = LoadSongFile(rhx_song_file_path);

        return null;
    }

    static string FindSongFile(string song_name)
    {
        List<string> lookup_paths = GameState.Variables.RHX_song_lookup_paths;
        for (int i = 0; i < lookup_paths.Count; ++i)
        {
            string s = lookup_paths[i];
            s = s.Replace("<data>", Application.dataPath);

            Log("RHX: [%]: %", i, s);

            if (!Directory.Exists(s) && LogE("This lookup directory does not exist: %", s))
                continue;

            string s_song_dir = "%/%".Parse(s, song_name);
            if (!Directory.Exists(s_song_dir) && LogE("The song was not found here: %", s_song_dir))
                continue;

            return s_song_dir;
        }

        return null;
    }

    static Song LoadSongFile(string file_path)
    {
        if (!File.Exists(file_path) && LogE("File does not exist: %", file_path)) return null;

        using (TextReader r = File.OpenText(file_path))
        {
            string text = r.ReadToEnd();
            List<RHX_Token> tokens = new RHX_Tokenizer(text).Tokenize();
            foreach (RHX_Token t in tokens)
            {
                Log("Type: % lhs: % rhs: %", t.type, t.lhs, t.rhs);
            }
        }


        return null;
    }


}

public enum RHX_Token_Type { Comment, Section, Variable, NoteDecl, CheckpointDecl }
public struct RHX_Token
{
    public RHX_Token(RHX_Token_Type type, string lhs = null, object rhs = null)
    {
        this.type = type;
        this.lhs = lhs;
        this.rhs = rhs;
    }

    public RHX_Token_Type type;
    public string lhs;
    public object rhs;
}

public class RHX_Tokenizer
{
    public RHX_Tokenizer(string text) { Text = text; c = text[0]; }

    public string Text;

    char c;
    int c_index;

    char Advance()
    {
        ++c_index;
        if (c_index >= Text.Length) return '\0';
        c = Text[c_index];
        return c;
    }
    char Peek(int count = 1)
    {
        if (c_index + count >= Text.Length) return '\0';
        return Text[c_index + count];
    }

    public List<RHX_Token> Tokenize()
    {
        List<RHX_Token> list = new List<RHX_Token>();

        while (c_index < Text.Length)
        {
            if (c.IsWhitespace()) { Advance(); continue; }

            switch (c)
            {
                case '/':
                    {
                        if (Peek() == '/') Advance();
                        if (Peek() == ' ') Advance();

                        string s = null;
                        while (Peek() != '\n' && c_index < Text.Length)
                            s += Advance();

                        list.Add(new RHX_Token(RHX_Token_Type.Comment, s));
                        break;
                    }
            }

            Advance();
        }

        return list;
    }
}