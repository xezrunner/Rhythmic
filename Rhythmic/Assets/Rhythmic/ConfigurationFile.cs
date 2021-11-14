using System;
using System.Collections.Generic;
using System.IO;
using static Logger;

public class ConfigurationFile
{
    public enum Entry_Type { Variable, List, Function, Command }
    public abstract class Entry
    {
        public string name;
        public Entry_Type type;
        public object value_obj;
        public Type value_type;
    }
    public class Entry<T> : Entry
    {
        public Entry(Entry_Type type, string name, T value)
        {
            this.type = type;
            this.name = name;
            this.value = value;
            value_obj = value;
            value_type = typeof(T);
        }
        public T value;
    }

    public abstract class Value { public object value_obj; }
    public class Value<T> : Value
    {
        public Value(T value) { this.value = value; value_obj = value; }
        public T value;
    }

    public ConfigurationFile(string path)
    {
        this.path = path;
        name = Path.GetFileName(path).RemoveExt();
        ReadFromPath(path);
    }

    public string name;
    public string path;

    public Dictionary<string, List<Entry>> directory;

    List<Token> tokens;
    int tokens_count;

    public static bool CONFIGFILE_DebugPrintTokens = false;

    public bool ReadFromPath(string path)
    {
        if (!File.Exists(path) && LogE("File does not exist: '%'".TM(this), path)) return false;

        // 1. Read in the file as a text file:
        string text = File.ReadAllText(path);
        if (text.Length <= 0 && LogE("File is empty: '%'".TM(this), path)) return false;

        // 2. Parse!
        tokens = new Parser(text).Parse();
        tokens_count = tokens.Count;

        // [DEBUG] print!
        if (CONFIGFILE_DebugPrintTokens)
        {
            int i = -1;
            Log("BEGIN TOKEN PRINT!  Length: %", tokens.Count);
            foreach (Token t in tokens)
            {
                string s = "[%]: Type: %".Parse(++i, t.type);
                if (t.type == Token_Type.Section || t.type == Token_Type.Identifier || t.type == Token_Type.Number || t.type == Token_Type.String || t.type == Token_Type.Comment)
                    s += "  Value: %".Parse(t.value);
                Log(s);
            }
        }

        // 3. Interpret:
        Interpret();

        return true;
    }

    void Interpret()
    {
        directory = new Dictionary<string, List<Entry>>();

        string sect_name = "none";
        List<Entry> sect_list = new List<Entry>();
        directory.Add(sect_name, sect_list);

        for (int i = 0; i < tokens_count; i++)
        {
            Token t = tokens[i];

            switch (t.type)
            {
                case Token_Type.Section:
                    {
                        sect_name = t.value;
                        sect_list = new List<Entry>();
                        directory.Add(sect_name, sect_list);
                        break;
                    }
                case Token_Type.Identifier:
                    {
                        Token t_next = null;
                        if (i + 1 < tokens_count) t_next = tokens[i + 1];

                        Entry entry = null;

                        switch (t_next.type)
                        {
                            case Token_Type.Newline:
                                LogW("Variable % requires a rhs assignment! Ignoring.", t.value); continue;
                            case Token_Type.Identifier:
                            case Token_Type.String:
                                entry = new Entry<string>(Entry_Type.Variable, t.value, t_next.value); break;
                            case Token_Type.Number:
                                {
                                    if (t_next.value.ContainsAny(',', '.'))
                                        entry = new Entry<float>(Entry_Type.Variable, t.value, t_next.value.ParseFloat());
                                    else
                                        entry = new Entry<int>(Entry_Type.Variable, t.value, t_next.value.ParseInt());
                                    break;
                                }
                            case Token_Type.OpenParen:
                            case Token_Type.OpenBrace: // Lists
                                {
                                    string name = t.value;
                                    t = tokens[i += 2];

                                    List<Value> values = new List<Value>();

                                    while (t.type != Token_Type.CloseParen && t.type != Token_Type.CloseBrace)
                                    {
                                        Value value = null;

                                        switch (t.type)
                                        {
                                            case Token_Type.Identifier:
                                            case Token_Type.String:
                                                value = new Value<string>(t.value); break;
                                            case Token_Type.Number:
                                                {
                                                    if (t.value.ContainsAny(',', '.'))
                                                        value = new Value<float>(t.value.ParseFloat());
                                                    else
                                                        value = new Value<int>(t.value.ParseInt());
                                                    break;
                                                }
                                        }

                                        values.Add(value);
                                        t = tokens[++i];
                                    }

                                    entry = new Entry<List<Value>>(Entry_Type.List, name, values);
                                    break;
                                }
                        }

                        if (entry != null)
                        {
                            sect_list.Add(entry);

                            // If we didn't just add a list, we can safely jump 2 tokens ahead (value + 1).
                            if (entry.type != Entry_Type.List) // TODO: This is weird.
                                i += 2;
                        }

                        break;
                    }
                case Token_Type.OpenParen: // Configuration-function
                    {
                        InterpretFunction(tokens, ref i);
                        break;
                    }
            }
        }
    }

    void InterpretFunction(List<Token> tokens, ref int i)
    {
        Token t = tokens[++i];
        string cmd = t.value;

        switch (cmd)
        {
            case "debugsystem":
                {
                    // TODO: Component arguments!
                    DebugSystem.CreateDebugSystemObject();
                    break;
                }

            default: // If this isn't a special function, call it as a console command.
                {
                    t = tokens[++i];
                    List<string> args = new List<string>();


                    while (t.type != Token_Type.CloseParen)
                    {
                        args.Add(t.value);
                        t = tokens[++i];
                    }

                    //sect_list.Add(new Entry<bool>(Entry_Type.Function, t.value, true));

                    string args_s = "";
                    if (args.Count > 0)
                    {
                        foreach (string s in args) args_s += " " + s;
                        args_s = args_s.Substring(1, args_s.Length - 1);
                    }

                    Log("%: Executing command '%' with args '%'.".TM(this),
                            name.AddColor(Colors.Unimportant), cmd.AddColor(Colors.Application), args_s.AddColor(Colors.Unimportant));
                    DebugConsole.ExecuteCommand(cmd, args.ToArray());
                    return; // EXIT!
                }
        }
    }

    public enum Token_Type { Comment, Section, Identifier, Number, String, OpenParen, CloseParen, OpenBrace, CloseBrace, Newline }
    public class Token
    {
        public Token(Token_Type type, string value = "")
        {
            this.type = type;
            this.value = value;
        }
        public Token_Type type;
        public string value;
    }
    public class Parser
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
                    case ' ': continue;
                    case '\r':
                    case '\t':
                    case '\n': /*t = new Token(Token_Type.Newline);*/ continue;

                    case '/': // Comments
                        {
                            string s = "";
                            while (!c.IsNewline() && pos < length - 1)
                            {
                                s += c;
                                c = text[++pos];
                            }
                            --pos;

                            t = new Token(Token_Type.Comment, s);
                            break;
                        }

                    case ':':
                        {
                            c = text[++pos]; // Advance from :
                            if (c == ' ') c = text[++pos]; // Advance from a space
                            string s = "";
                            while (!c.IsNewline() && pos < length - 1)
                            {
                                s += c;
                                c = text[++pos];
                            }
                            --pos;

                            t = new Token(Token_Type.Section, s);
                            break;
                        }

                    case '(': t = new Token(Token_Type.OpenParen); break;
                    case ')': t = new Token(Token_Type.CloseParen); break;
                    case '{': t = new Token(Token_Type.OpenBrace); break;
                    case '}': t = new Token(Token_Type.CloseBrace); break;

                    case '"':
                        {
                            c = text[++pos]; // Advance from "
                            string s = "";
                            while (c != '"' && pos < length - 1)
                            {
                                s += c;
                                c = text[++pos];
                            }
                            --pos;

                            t = new Token(Token_Type.String, s);
                            break;
                        }

                    case char x when char.IsNumber(x):
                        {
                            string s = "";
                            while (!c.IsWhitespace() && c != ')' && c != '}' && pos < length - 1)
                            {
                                s += c;
                                c = text[++pos];
                            }
                            --pos;

                            t = new Token(Token_Type.Number, s);
                            break;
                        }

                    default: // Identifier
                        {
                            string s = "";
                            while (!c.IsWhitespace() && c != ')' && c != '}' && pos < length - 1)
                            {
                                s += c;
                                c = text[++pos];
                            }
                            --pos;

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