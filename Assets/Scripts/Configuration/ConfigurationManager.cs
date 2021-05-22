using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ConfigurationManager
{
    public static string config_extension = ".r_config";
    public static string config_basefolder = Application.dataPath;
    public static string[] config_folders = new string[] { "Configuration", "Config" };

    public static bool enable_hotloading = true;

    public List<Configuration> Configurations = new List<Configuration>();

    string LookupFile(string file_name)
    {
        foreach (string s in config_folders)
        {
            string path = Path.Combine(config_basefolder, s, file_name);
            if (File.Exists(path)) return path;
        }
        return "";
    }

    public Configuration LoadConfiguration(string file_name)
    {
        if (!file_name.EndsWith(config_extension))
            file_name += config_extension;

        string file_path = LookupFile(file_name);

        if (!File.Exists(file_path))
        {
            Logger.LogConsoleE("Could not load configuration % - it doesn't exist!", file_name);
            return null;
        }

        // TODO: Performance? Files could be large! (?)
        // TODO: Error checking!
        Tokenizer tokenizer;
        using (TextReader reader = File.OpenText(file_path))
        {
            string text = reader.ReadToEnd();
            tokenizer = new Tokenizer(text);
        }

        Configuration config = ParseConfigurationFile(tokenizer);
        return config;
    }

    public List<Token> Tokens = new List<Token>();
    public static bool debug_tokens = true;
    public Configuration ParseConfigurationFile(Tokenizer t)
    {
        // Grab tokens:
        while (!t.end_of_file)
        {
            Token token = GetToken(t);
            if (debug_tokens) Logger.LogConsole("Type: % | Value: %", token.Type.ToString(), token.Value);

            // TODO: Duplication check?
            Tokens.Add(token);
        }

        // Load stuff:
        {
            
        }

        return null;
    }

    #region Token parsing

    public enum Token_Type { EndOfFile, Unknown, Comment, Section, Identifier, Number, OpenParen, CloseParen, String }
    public class Token
    {
        public Token(Token_Type type = Token_Type.Unknown) { Type = type; }
        public Token(Token_Type type, string value) { Type = type; Value = value; }

        public Token_Type Type;
        public string Value;
    }
    public class Tokenizer
    {
        public Tokenizer(string t) { Text = t; c = Text[0]; }
        public Tokenizer(string t, int p) { Text = t; cursor = p; c = Text[p]; }

        public char Advance()
        {
            if (cursor + 1 < Text.Length)
                return c = Text[++cursor];
            else { end_of_file = true; return '\0'; }
        }
        public void Backwards()
        {
            if (cursor > 0)
                c = Text[--cursor];
        }
        public char Peek()
        {
            char c = Advance();
            Backwards();
            return c;
        }

        public string Text;
        public char c;
        public int cursor;
        public bool end_of_file;
    }

    public Token GetToken(Tokenizer t)
    {
        // TODO: We should really use a StringBuilder for performance reasons, especially
        // if we're going to be hotloading some files!

        Token token = null;
        if (t.end_of_file) return token;

        while (!t.end_of_file && t.c.IsWhitespace()) t.Advance();
        switch (t.c)
        {
            case '#':
                {
                    token = new Token(Token_Type.Comment);
                    while (!t.end_of_file && (t.c != '\r' && t.c != '\n'))
                    {
                        token.Value += t.c; // Include the entire comment starting from #
                        t.Advance();
                    }
                    break;
                }
            case '(': token = new Token(Token_Type.OpenParen); break;
            case ')': token = new Token(Token_Type.CloseParen); break;
            case '"':
                {
                    token = new Token(Token_Type.String);
                    while (!t.end_of_file && t.c != '"')
                        token.Value += t.Advance();
                    break;
                }
            case ':':
                {
                    token = new Token(Token_Type.Section);
                    if (t.Peek() == ' ') t.Advance();
                    while (!t.end_of_file && !t.c.IsNewline())
                        token.Value += t.Advance();
                    break;
                }
            case char x when (char.IsLetter(x)):
                {
                    string ident = "";
                    while (!t.end_of_file && !t.c.IsWhitespace() && t.c != ')')
                    {
                        ident += t.c;
                        t.Advance();
                    }
                    token = new Token(Token_Type.Identifier, ident);

                    if (t.c == ')') t.Backwards();
                    break;
                }
            case char x when (char.IsDigit(x) || x == '-' || x == '.'):
                {
                    string text = "";
                    while (!t.end_of_file && !t.c.IsWhitespace() && t.c != ')')
                    {
                        text += t.c;
                        t.Advance();
                    }
                    token = new Token(Token_Type.Number, text);

                    if (t.c == ')') t.Backwards(); // // Traverse -1 to Leave ')' to be parsed
                    break;
                }
            default:
                token = new Token(Token_Type.Unknown); break;
        }

        t.Advance();
        return token;
    }

    #endregion

    // -------------------- //

    public static void DEBUG_TestConfig()
    {
        ConfigurationManager m = new ConfigurationManager();
        m.LoadConfiguration("test");
    }

    public static void DEBUG_RuntimeTestConfig()
    {
        Configuration c = new Configuration();
        c.AddVariable("test", "This is a test value.");
        c.AddVariable("test_int", 0);

        Logger.LogConsole("Test printing items from runtime test conf: ");
        for (int i = 0; i < c.Variables.Count; ++i)
        {
            KeyValuePair<string, object> it = c.Variables.ElementAt(i);
            Logger.LogConsole("[%] %: %", i, it.Key, it.Value);
        }
        Logger.LogConsole("");
    }
}