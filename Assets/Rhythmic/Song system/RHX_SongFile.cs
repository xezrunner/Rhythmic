using System.Collections.Generic;
using System.IO;
using static Logger;

public struct RHX_NoteDecl
{
    public int side; // TODO: enum (?)
    public int type; // TODO: enum!
    public float time; // TODO: timeunit!
}

public struct RHX_TrackDecl
{
    public string name;
    public List<RHX_NoteDecl> notes;
}

public class RHX_SongFile
{
    public RHX_SongFile(string file_path)
    {
        this.file_path = file_path;
        Parse();
        Interpret();
    }

    public string file_path;

    public List<RHX_Token> Tokens;
    public int tokens_count;


    public void Parse()
    {
        if (file_path.IsEmpty() && LogE("File path is empty!".TM(this))) return;

        using (TextReader r = File.OpenText(file_path))
        {
            string text = r.ReadToEnd();
            RHX_SongFile_Parser parser = new RHX_SongFile_Parser(text);
            Tokens = parser.Tokenize();
            tokens_count = Tokens.Count;
        }
    }

    public static bool RHXFILE_DebugLogTokens = true;
    public static bool RHXFILE_DebugLogComments = false;

    // --------------- //

    public string name;
    public string friendly_name;

    public float bpm;
    public float tunnel_scale = 1.0f;

    public void Interpret()
    {

    }
}

public enum RHX_Token_Type { Comment, Section, Identifier, Number, OpenBrace, CloseBrace, NoteDecl, CheckpointDecl }
public struct RHX_Token
{
    public RHX_Token(RHX_Token_Type type, string value = null)
    {
        this.type = type;
        this.value = value;
    }

    public RHX_Token_Type type;
    public string value;
}
public class RHX_SongFile_Parser
{
    public RHX_SongFile_Parser(string text) { Text = text; c = text[0]; }

    public string Text;

    char c;
    int c_index;

    char Advance(bool ret_prev = false)
    {
        ++c_index;
        if (c_index >= Text.Length) { c = '\0'; return '\0'; }
        c = Text[c_index];

        if (!ret_prev) return Text[c_index - 1];
        return c;
    }
    char Backwards()
    {
        --c_index;
        if (c_index < 0) return '\0';
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
                        // TODO: We'll have to figure out a way to check for the end of the string.
                        // Perhaps we could use our own string operations?
                        while (Peek() != '\n' && c_index < Text.Length)
                            s += Advance(false);

                        list.Add(new RHX_Token(RHX_Token_Type.Comment, s));
                        break;
                    }
                // Declaration:
                case ':':
                    {
                        if (Peek() == ' ') Advance();

                        string s = null;
                        while (!c.IsWhitespace() && c_index < Text.Length)
                            s += Advance();

                        list.Add(new RHX_Token(RHX_Token_Type.Section, s));
                        break;
                    }
                case '{': list.Add(new RHX_Token(RHX_Token_Type.OpenBrace)); break;
                case '}': list.Add(new RHX_Token(RHX_Token_Type.CloseBrace)); break;
                case char x: // || x == '-' || x == '.' for numbers
                    {
                        string s = null;

                        bool is_letter = char.IsLetter(x);

                        while (c != '\0' && !c.IsWhitespace() && (c != ' ' && c != '\n'))
                        {
                            if (!is_letter && !char.IsDigit(c) && c != '.' && c != ',') break;
                            s += Advance();
                        }

                        if (c == ' ' || c == '\n') Backwards();

                        RHX_Token t = new RHX_Token(char.IsLetter(x) ? RHX_Token_Type.Identifier : RHX_Token_Type.Number, s);
                        list.Add(t);
                        break;
                    }
            }

            Advance();
        }

        return list;
    }
}

/*
public static string FindSongFile(string song_name)
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
*/