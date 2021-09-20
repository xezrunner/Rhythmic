using System.Collections.Generic;

public class AMP_MoggSong
{

}

public enum AMP_MoggSong_Token_Type { Unknown = -1, Comment, Identifier, Number, OpenParen, CloseParen, OpenBrace, CloseBrace, StringQuotes, TimeUnitColon }
public struct AMP_MoggSong_Token
{
    public string lhs;
    public object rhs;
}

public class AMP_MoggSong_Parser
{
    public AMP_MoggSong_Parser(string text) { Text = text; }

    public string Text;

    public List<AMP_MoggSong_Token> Tokenize()
    {
        List<AMP_MoggSong_Token> list = new List<AMP_MoggSong_Token>();

        return null;
    }
}