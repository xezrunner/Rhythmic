using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class MoggSong : MonoBehaviour
{
    public SongController SongController { get { return SongController.Instance; } }

    public static MoggSong Instance;

    // Song properties
    public int songLengthInMeasures;
    public int songCountInTime;
    public List<string> songTracks = new List<string>();
    public int songBpm;

    // TODO: Mixer!
    // TODO: Score goals!

    // Fudge Factor
    // If a song doesn't have a fudge factor defined, assume it's 0.
    float? _songFudgeFactor;
    public float songFudgeFactor { get { if (!_songFudgeFactor.HasValue) return 1f; else return _songFudgeFactor.Value; } set { _songFudgeFactor = value; } }

    // TOOD: Unknown props
    public int[] songEnableOrder;
    public int[] songSectionStartBars;

    // TODO: Metadata!

    // Song boss level
    // n + 1 streaks required
    public int songBossLevel;

    void Awake() => Instance = this;

    List<Token> Tokens = new List<Token>();
    public void LoadMoggSong(string songName)
    {
        string finalPath = AmplitudeGame.AMP_GetSongFilePath(songName, AmplitudeGame.AMP_FileExtension.moggsong); // moggsong path

        // TODO: error checking
        Tokenizer tokenizer;
        using (TextReader reader = File.OpenText(finalPath))
        {
            string text = reader.ReadToEnd();
            tokenizer = new Tokenizer(text);
        }

        while (true) // TODO: This could end up being an infinite-loop!
        {
            Token token = GetToken(tokenizer);
            Tokens.Add(token);
            if (token.Type == Token_Type.EndOfFile) break;
        }

        // Parse information from tokens:
        // TODO: Move this to its own proc?
        for (int i = 0; i < Tokens.Count; ++i)
        {
            Token token = Tokens[i];
            switch (token.Text)
            {
                case "length":
                    {
                        Token token_value = Tokens[++i];

                        // Get the measure count from the time unit (MMM:_:_)
                        string[] time_values = token_value.Text.Split(':');
                        songLengthInMeasures = time_values[0].ParseInt();

                        break;
                    }
                case "countin": songCountInTime = SetValue<int>(Tokens, ++i); break;
                case "tunnel_scale": songFudgeFactor = SetValue<float>(Tokens, ++i); break; /// TODO: .xx values (without the leading 0) DO NOT WORK! FIX!!
                case "bpm": songBpm = SetValue<int>(Tokens, ++i); break;
                case "boss_level": songBossLevel = SetValue<int>(Tokens, ++i); break;
                    // TODO: freestyle tracks were previously added here. This might be the MIDI reading's job.
            }
        }
    }

    public void DebugPrintTokens()
    {
        if (!SongController.Instance.IsEnabled)
        {
            DebugConsole.Log("SongController is not enabled - this could be due to an invalid song being specified. ".AddColor(Colors.Warning) + "[Song name: %]", SongController.songName);
            return;
        }

        int type_max_width = Tokens.Max(s => s.Type.ToString().Length) + 1;

        for (int i = 0; i < Tokens.Count; ++i)
        {
            string token_type = Tokens[i].Type == Token_Type.Unknown ? "---" : Tokens[i].Type.ToString();
            token_type = token_type.AlignSpaces_New(13, type_max_width, false);
            switch (Tokens[i].Type) // Color coding
            {
                case Token_Type.OpenParen:
                case Token_Type.CloseParen:
                case Token_Type.OpenBrace:
                case Token_Type.CloseBrace:
                case Token_Type.StringQuotes: token_type = token_type.AddColor(Colors.IO); break;
                case Token_Type.Identifier:
                case Token_Type.Number:
                case Token_Type.TimeUnitColon: token_type = token_type.AddColor(Colors.Network); break;
                default:
                    token_type = token_type.AddColor(Colors.Info); break;
            }

            string token_text = Tokens[i].Text;
            switch (Tokens[i].Type)
            {
                case Token_Type.OpenParen: token_text = "("; break;
                case Token_Type.CloseParen: token_text = ")"; break;
                case Token_Type.OpenBrace: token_text = "{"; break;
                case Token_Type.CloseBrace: token_text = "}"; break;
                case Token_Type.TimeUnitColon: token_text = ":"; break;
                case Token_Type.EndOfFile: token_text = "EOF"; break;
            }

            DebugConsole.Log("[%]: Type: %"
                      + " | " + (token_text == "" || token_text == null ? null : "%").AddColor(Colors.Unimportant),
                      i.ToString("D3"), token_type, token_text);
        }
    }

    /// TODO: separate parsing into a different (partial) class?

    /// <summary>Returns a specific type of value from a token. <br/>
    /// Used in setting the values parsed from the .moggsong file. </summary>
    T SetValue<T>(List<Token> tokens, int index)
    {
        string value = tokens[index].Text;
        object return_value = null;

        switch (typeof(T))
        {
            case Type i when i == typeof(int): return_value = value.ParseInt(); break;
            case Type i when i == typeof(float): return_value = value.ParseFloat(); break;
            case Type i when i == typeof(bool): return_value = value.ParseBool(); break;
            case Type i when i == typeof(string): return_value = value; break;
            default:
                {
                    Logger.LogW("Weird type being set | value: % | target type: %".TM(this), tokens[index].Text, typeof(T).Name);
                    try { return_value = (T)Convert.ChangeType(value, typeof(T)); }
                    catch (Exception ex) { Logger.LogE("Failed to convert value to type % | %".TM(this), (typeof(T).Name), ex.Message); }
                    break;
                }
        }

        return (T)return_value;
    }

    public enum Token_Type { EndOfFile, Unknown, Comment, Identifier, Number, OpenParen, CloseParen, OpenBrace, CloseBrace, StringQuotes, TimeUnitColon }

    public class Token
    {
        public Token(Token_Type type = Token_Type.Unknown) { Type = type; }
        public Token(Token_Type type, string text) { Type = type; Text = text; }

        public Token_Type Type;
        public string Text;
    }
    public class Tokenizer
    {
        public Tokenizer(string t) { Text = t; c = Text[0]; }
        public Tokenizer(string t, int p) { Text = t; c = Text[pos]; pos = p; }

        public void Advance()
        {
            if (pos + 1 < Text.Length)
                c = Text[++pos];
            else end_of_file = true;
        }
        public void Backwards()
        {
            if (pos > 0) c = Text[--pos];
        }

        public string Text;
        public char c;
        public int pos;
        public bool end_of_file = false;
    }

    bool IsWhitespace(char c) => (c == ' ' || c == '\r' || c == '\n' || c == '\t');

    Token GetToken(Tokenizer t) // TODO: naming - e? t?
    {
        if (t.end_of_file) return new Token(Token_Type.EndOfFile);

        Token token;
        bool advance = true; // TODO: Not sure if we should use this, or t.Backwards() once, when we need to.

        while (!t.end_of_file && IsWhitespace(t.c)) t.Advance();
        switch (t.c)
        {
            case ';':
                {
                    token = new Token(Token_Type.Comment);

                    while (!t.end_of_file && t.c != '\r' && t.c != '\n')
                    {
                        token.Text += t.c;
                        t.Advance();
                    }

                    break;
                }
            case '(': token = new Token(Token_Type.OpenParen); break;
            case ')': token = new Token(Token_Type.CloseParen); break;
            case '{': token = new Token(Token_Type.OpenBrace); break;
            case '}': token = new Token(Token_Type.CloseBrace); break;
            case '"': token = new Token(Token_Type.StringQuotes); token.Text = "\""; break; // TODO: string reading!
            case ':': token = new Token(Token_Type.TimeUnitColon); break;
            case char x when char.IsLetter(x): // Letter:
                {
                    string text = "";
                    while (!t.end_of_file && !IsWhitespace(t.c) && t.c != ')')
                    {
                        text += t.c;
                        t.Advance();
                    }

                    token = new Token(Token_Type.Identifier, text);

                    // TODO: decide whether to advance or t.Backwards()
                    //if (t.c == ')') advance = false; // Do not advance on ')', because we want it as an identifier!
                    if (t.c == ')') t.Backwards(); // // Traverse -1 to Leave ')' to be parsed
                    break;
                }
            case char x when (char.IsDigit(x) || x == '-' || x == '.'): // Number:
                {
                    string text = "";
                    //bool is_negative = (t.c == '-');
                    //bool is_fraction = (t.c == '.' || t.c == ',');

                    while (!t.end_of_file && !IsWhitespace(t.c) && t.c != ')')
                    {
                        text += t.c;
                        t.Advance();
                    }

                    token = new Token(Token_Type.Number, text);

                    if (t.c == ')') t.Backwards(); // // Traverse -1 to Leave ')' to be parsed
                    break;
                }
            default: // Identifier
                {
                    token = new Token(Token_Type.Unknown); if (!IsWhitespace(t.c)) token.Text += $"'{t.c}'";
                    break;
                }
        }

        if (advance) t.Advance();
        return token;
    }
}