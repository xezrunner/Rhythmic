using System;
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
            string path = Path.Combine(config_basefolder, s);
            if (!Directory.Exists(path)) continue;

            string file_path = Path.Combine(path, file_name);

            if (File.Exists(file_path)) return file_path;

            string[] dirs = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
            foreach (string subdir in dirs)
            {
                string subdir_file_path = Path.Combine(subdir, file_name);
                if (File.Exists(subdir_file_path)) return subdir_file_path;
            }
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

        List<Token> tokens = ParseConfigurationFile(tokenizer);
        if (tokens.Count <= 0)
        {
            Logger.LogConsoleE("An error occured while trying to parse the config file '%'.", file_name);
            return null;
        }

        Configuration config = InterpretConfigurationFile(file_name, tokens); // new Configuration(file_name, config_name)

        return config;
    }

    public static bool debug_tokens = false;
    public List<Token> ParseConfigurationFile(Tokenizer t)
    {
        List<Token> Tokens = new List<Token>();

        // Grab tokens:
        while (!t.end_of_file)
        {
            Token token = GetToken(t);
            if (debug_tokens) Logger.LogConsole("Type: % | Value: %", token.Type.ToString(), token.Value);

            // TODO: Duplication check?
            Tokens.Add(token);
        }

        return Tokens;
    }

    public Configuration InterpretConfigurationFile(string file_name, List<Token> tokens)
    {
        Configuration config = new Configuration(file_name);
        int cursor = 0, total = tokens.Count;
        Token token = tokens[cursor];

        string last_section = Configuration.SECTION_GLOBAL;

        while (cursor < total)
        {
            switch (token.Type)
            {
                case Token_Type.Meta_Identifier:
                    {
                        string s = token.Value;
                        if (s.IsEmpty())
                        {
                            Logger.LogConsoleW("Configuration: Meta_Identifier with no value! ('%')", file_name);
                            break;
                        }
                        bool value = s[0] != '!';
                        if (!value) s.Substring(1, s.Length - 1); // Remove '!' from string

                        if (s == "local") config.is_local = value;
                        if (s == "hotreload") config.is_hotreload = value;
                        if (s == "name")
                        {
                            token = tokens[++cursor];
                            if (token.Type == Token_Type.Identifier)
                                config.config_name = token.Value;
                        }

                        break;
                    }

                case Token_Type.Section:
                    {
                        string s = token.Value;
                        if (s.IsEmpty())
                            s = Configuration.SECTION_GLOBAL;

                        config.AddSection(s);
                        last_section = s;
                        break;
                    }
                case Token_Type.Identifier:
                    {
                        string name = token.Value;
                        // Advance to the next token.
                        if ((cursor + 1) < total) token = tokens[++cursor];

                        string value = null;
                        if (token.Type == Token_Type.Identifier || token.Type == Token_Type.Number || token.Type == Token_Type.String)
                            value = token.Value;
                        else if (token.Type == Token_Type.OpenParen)
                        {
                            string s = "(";
                            while (token.Type != Token_Type.CloseParen)
                            {
                                token = tokens[++cursor]; // TODO: should we bounds-check everywhere?
                                if (token.Type == Token_Type.Identifier || token.Type == Token_Type.Number || token.Type == Token_Type.String)
                                    s += token.Value + ", ";
                            }
                            s = s.Remove(s.Length - 2, 2) + ')';

                            value = s;
                            //Logger.LogConsoleW("Configuration: Multi-values are not yet supported! ('%')", file_name);
                        }

                        config.AddVariable(last_section, name, value);
                        break;
                    }
            }
            if (cursor + 1 < total) token = tokens[++cursor];
            else break;
        }
        return config;
    }

    #region Token parsing

    public enum Token_Type { EndOfFile, Unknown, Comment, Meta_Identifier, Section, Identifier, Number, OpenParen, CloseParen, String }
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

        /// <summary>
        /// If <paramref name="return_next"/> is set to true, the returned character will be the one you advance to.<br/>
        /// If false, the returned character will be the one you were standing on prior to advancing.
        /// </summary>
        public char Advance(bool return_next = true)
        {
            if (cursor + 1 < Text.Length)
                c = Text[++cursor];
            else { end_of_file = true; return '\0'; }

            if (return_next) return c;
            else if (cursor > 0) return Text[cursor - 1];
            else return '\0';
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

    static bool parser_include_string_quotes = false;
    public Token GetToken(Tokenizer t)
    {
        // TODO: We should really use a StringBuilder for performance reasons, especially
        // if we're going to be hotloading some files!

        Token token = null;
        if (t.end_of_file) return token;

        while (!t.end_of_file && t.c.IsWhitespace()) t.Advance();
        switch (t.c)
        {
            case '/':
                {
                    token = new Token(Token_Type.Comment);
                    while (!t.end_of_file && (t.c != '\r' && t.c != '\n'))
                    {
                        token.Value += t.c; // Include comment marks
                        t.Advance();
                    }
                    break;
                }
            case '#':
                {
                    token = new Token(Token_Type.Meta_Identifier);
                    t.Advance(); // Skip '#'
                    if (t.c == ' ') t.Advance(); // Skip spaces
                    while (!t.end_of_file && /*!t.c.IsNewline()*/ !t.c.IsWhitespace())
                        token.Value += t.Advance(false);
                    break;
                }
            case '(': token = new Token(Token_Type.OpenParen); break;
            case ')': token = new Token(Token_Type.CloseParen); break;
            case '"':
                {
                    token = new Token(Token_Type.String);
                    int pos = 0;
                    while (!t.end_of_file)
                    {
                        if (t.c == '"')
                        {
                            if (parser_include_string_quotes) token.Value += t.Advance(false);
                            else t.Advance();

                            if (pos > 0) break;
                        }
                        else
                            token.Value += t.Advance(false);

                        ++pos;
                    }
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

    public static Configuration DEBUG_TestConfig()
    {
        ConfigurationManager m = new ConfigurationManager();
        return m.LoadConfiguration("test");
    }

    public static void DEBUG_RuntimeTestConfig()
    {
        /*
        Configuration c = new Configuration();
        c.AddVariable("test", "This is a test value.");
        c.AddVariable("test_int", 0);

        Logger.LogConsole("Test printing items from runtime test conf: ");
        for (int i = 0; i < c.Sections.Count; ++i)
        {
            KeyValuePair<string, object> it = c.Sections.ElementAt(i);
            Logger.LogConsole("[%] %: %", i, it.Key, it.Value);
        }
        Logger.LogConsole("");
        */
    }
}