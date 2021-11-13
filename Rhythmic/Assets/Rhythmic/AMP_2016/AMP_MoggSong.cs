using System.Collections.Generic;
using System.IO;
using static Logger;

// Fields:

public partial class AMP_MoggSong
{
    public string mogg_path;
    public string midi_path;

    public int length; // in bars | TODO: this might need a time unit! (format is x:y:z <- what do these mean?)
    public int countin;

    public List<MoggSong_TrackDef> tracks;

    public List<float[]> pans;
    public List<float[]> vols;

    public List<float> active_track_db;

    public string arena_path;
    public List<float[]> score_goal;

    public float tunnel_scale = 1.0f;

    public List<int> enable_order;
    public List<int> section_start_bars;

    // Metadata:
    public struct sect_song_metadata
    {
        public string title;
        public string artist;
        public string artist_short;
        public string unlock_requirement; // TODO: enum?
        public string desc; // TODO: manual lookup?
        public float bpm; // int?
    }
    public sect_song_metadata song_metadata;

    public int preview_start_ms;
    public int preview_length_ms;
}
public struct MoggSong_TrackDef
{
    public string name;
    public int[] audio_ids;
    public string _bus; // not needed!
}

// Functionality (parsing):

public partial class AMP_MoggSong
{
    public AMP_MoggSong(string path) { ReadFromPath(path); }

    public AMP_MoggSong ReadFromPath(string path)
    {
        if (!File.Exists(path) && LogE("File does not exist: '%'".TM(this), path)) return null;


        // 1. Read in the moggsong as a text file:
        string text = File.ReadAllText(path);
        if (text.Length <= 0 && LogE("File is empty: '%'".TM(this), path)) return null;

        // 2. Parse!
        List<Token> tokens = new Parser(text).Parse();

        // [DEBUG] print!
        int i = -1;
        Log("BEGIN TOKEN PRINT!  Length: %", tokens.Count);
        foreach (Token t in tokens)
        {
            string s = "[%]: Type: %".Parse(++i, t.type);
            if (t.type == Token_Type.Identifier || t.type == Token_Type.Number || t.type == Token_Type.String || t.type == Token_Type.Comment)
                s += "  Value: %".Parse(t.value);
            Log(s);
        }

        return null;
    }

    /// - Consider both ':' and ';' as comments!
    enum Token_Type { Comment, Identifier, String, Number, OpenParen, CloseParen, OpenBrace, CloseBrace }
    class Token
    {
        public Token(Token_Type type, string value = null)
        {
            this.type = type;
            this.value = value;
        }
        public Token_Type type;
        public string value = null;
    }
    class Parser
    {
        public Parser(string text)
        {
            this.text = text;
            length = text.Length;
        }

        string text;
        int length;

        char c;
        int pos = -1;

        public List<Token> Parse()
        {
            List<Token> list = new List<Token>();

            while (pos < length - 1)
            {
                Token t = null;
                c = text[++pos];

                switch (c)
                {
                    case ' ':
                    case '\r':
                    case '\n': continue;

                    case ':':
                    case ';':
                        {
                            string comment = "";
                            while (!c.IsNewline() && pos < length)
                            {
                                comment += c; // Include comment char as well.
                                c = text[++pos];
                            }
                            t = new Token(Token_Type.Comment, "");
                            break;
                        }

                    case '(': t = new Token(Token_Type.OpenParen); break;
                    case ')': t = new Token(Token_Type.CloseParen); break;
                    case '{': t = new Token(Token_Type.OpenBrace); break;
                    case '}': t = new Token(Token_Type.CloseBrace); break;

                    case '"':
                        {
                            string s = "";
                            c = text[++pos]; // Advance from "
                            while (c != '"' && pos < length)
                            {
                                s += c;
                                c = text[++pos];
                            }
                            t = new Token(Token_Type.String, s);
                            break;
                        }

                    case char x when char.IsNumber(x):
                        {
                            string s = "";
                            while (!c.IsWhitespace() && c != ')' && pos < length)
                            {
                                s += c;
                                c = text[++pos];
                            }
                            t = new Token(Token_Type.Number, s);
                            break;
                        }

                    default: // Identifier
                        {
                            string s = "";
                            while (!c.IsWhitespace() && c != ')' && pos < length)
                            {
                                s += c;
                                c = text[++pos];
                            }
                            t = new Token(Token_Type.Identifier, s);
                            break;
                        }
                }

                if (t != null) list.Add(t);
            }

            return list;
        }
    }
}