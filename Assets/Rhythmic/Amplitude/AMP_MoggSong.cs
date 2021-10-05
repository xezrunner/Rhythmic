using System;
using System.Collections.Generic;
using System.IO;
using static Logger;

public class AMP_MoggSong
{
    public AMP_MoggSong(string file_path)
    {
        this.file_path = file_path;
        Parse();
        Interpret();
    }

    public string file_path;

    public List<MoggSong_Token> Tokens;
    public int tokens_count;

    public void Parse()
    {
        if (file_path.IsEmpty() && LogE("File path is empty!".TM(this))) return;

        using (TextReader r = File.OpenText(file_path))
        {
            string text = r.ReadToEnd();
            AMP_MoggSong_Parser parser = new AMP_MoggSong_Parser(text);
            Tokens = parser.Tokenize();
            tokens_count = Tokens.Count;
        }
    }

    public static bool MOGGSONG_DebugLogTokens = false;
    public static bool MOGGSONG_DebugLogComments = false;

    public string song_name;
    public string friendly_name;
    public int length_bars;
    public int countin_bars;
    public float tunnel_scale;
    public float bpm;
    public int boss_level;

    public void Interpret()
    {
        if (Tokens == null || Tokens.Count == 0) { LogE("Empty tokens!".TM(this)); return; }

        for (int i = 0; i < tokens_count; ++i)
        {
            MoggSong_Token t0 = Tokens[i];
            MoggSong_Token t1 = (i + 1) < tokens_count ? Tokens[i + 1] : new MoggSong_Token(MoggSong_Token_Type.Unknown);

            if (MOGGSONG_DebugLogTokens)
            {
                if (t0.type == MoggSong_Token_Type.Comment && !MOGGSONG_DebugLogComments) continue;

                string s = "moggsong: [%]: Type: %";
                if (!t0.value.IsEmpty()) s += " | Value: '%'";

                Log(s, i, t0.type, t0.value);
            }

            // TODO: READ THESE BETTER!!!

            if (t0.value == "mogg_path")         song_name     = t1.value.RemoveExt();   
            else if (t0.value == "title")        friendly_name = t1.value;
            else if (t0.value == "length")       length_bars   = t1.value.ParseInt();
            else if (t0.value == "countin")      countin_bars  = t1.value.ParseInt();
            else if (t0.value == "tunnel_scale") tunnel_scale  = t1.value.ParseFloat();
            else if (t0.value == "bpm")          bpm           = t1.value.ParseFloat();
            else if (t0.value == "boss_level")   boss_level    = t1.value.ParseInt();
            else if (t0.value == "boss_level")   boss_level    = t1.value.ParseInt();
        }
    }
}

public enum MoggSong_Token_Type { Unknown = -1, Comment, Identifier, Number, OpenParen, CloseParen, OpenBrace, CloseBrace, StringQuotes, TimeUnitColon }
public struct MoggSong_Token
{
    /*
    public MoggSong_Token(MoggSong_Token_Type type, string lhs = null, string rhs = null)
    {
        this.type = type;
        this.lhs = lhs;
        this.rhs = rhs;
    }
    */

    public MoggSong_Token(MoggSong_Token_Type type, string value = null)
    {
        this.type = type;
        this.value = value;
    }

    public MoggSong_Token_Type type;
    //public string lhs;
    //public string rhs;
    public string value;
}

public class AMP_MoggSong_Parser
{
    public AMP_MoggSong_Parser(string text)
    {
        if (text.IsEmpty()) throw new Exception("Text to tokenize was empty!".T(this));
        Text = text; c = text[0];
    }

    public string Text;

    char c;
    int c_index;

    // TODO: revise this - we shouldn't be this unsafe:ű
    /// <param name="ret_prev">Whether to return the previous character instead of the advanced one.</param>
    char Advance(bool ret_prev = false)
    {
        ++c_index;
        if (c_index >= Text.Length) return '\0';
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

    public List<MoggSong_Token> Tokenize()
    {
        List<MoggSong_Token> list = new List<MoggSong_Token>();

        while (c_index < Text.Length)
        {
            if (c.IsWhitespace()) { Advance(); continue; }

            MoggSong_Token t = new MoggSong_Token(MoggSong_Token_Type.Unknown);

            switch (c)
            {
                // Comments: 
                case ';':
                // case ':': // TEMP
                    {
                        string s = null;
                        while (Peek() != '\n' && c_index < Text.Length)
                            s += Advance();

                        t = new MoggSong_Token(MoggSong_Token_Type.Comment, s);
                        break;
                    }
                // NOTE: Moggsongs/DTAs seem to actually be LISP, or an internal Harmonix language very similar to/based on LISP.
                // It might be possible that things could be declared in LISP without being enclosed in parentheses.
                case '(': t = new MoggSong_Token(MoggSong_Token_Type.OpenParen);     break;
                case ')': t = new MoggSong_Token(MoggSong_Token_Type.CloseParen);    break;
                case '{': t = new MoggSong_Token(MoggSong_Token_Type.OpenBrace);     break;
                case '}': t = new MoggSong_Token(MoggSong_Token_Type.CloseBrace);    break;
                case ':': t = new MoggSong_Token(MoggSong_Token_Type.TimeUnitColon); break;
                case char x: // || x == '-' || x == '.' for numbers
                    {
                        string s = null;

                        bool is_letter = char.IsLetter(x);

                        while (!c.IsWhitespace() && c != ')')
                        {
                            if (!is_letter && !char.IsDigit(c) && c != '.' && c != ',') break;
                            s += Advance();
                        }

                        if (c == ')') Backwards();

                        t = new MoggSong_Token(char.IsLetter(x) ? MoggSong_Token_Type.Identifier : MoggSong_Token_Type.Number, s);
                        break;
                    }

            }

            list.Add(t);
            Advance();
        }


        return list;
    }
}