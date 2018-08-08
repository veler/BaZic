using System.ComponentModel;

namespace BaZic.Runtime.BaZic.Code.Lexer.Tokens
{
    /// <summary>
    /// Defines types of known token that can be detected by the lexer
    /// </summary>
    public enum TokenType
    {
        [Description("Not defined")]
        NotDefined,

        [Description(" ")]
        Whitespace,

        [Description("Start code")]
        StartCode,

        [Description("End code")]
        EndCode,

        [Description("+")]
        Plus,

        [Description("-")]
        Minus,

        [Description("*")]
        Asterisk,

        [Description("/")]
        Slash,

        [Description("%")]
        Percent,

        [Description(">")]
        GreaterThan,

        [Description("<")]
        LesserThan,

        [Description("=")]
        Equal,

        [Description("[")]
        LeftBracket,

        [Description("]")]
        RightBracket,

        [Description("(")]
        LeftParenth,

        [Description(")")]
        RightParenth,

        [Description("Comment")]
        Comment,

        [Description(",")]
        Comma,

        [Description(".")]
        Dot,

        [Description("New line")]
        NewLine,

        [Description("Identifier")]
        Identifier,

        [Description("VARIABLE")]
        Variable,

        [Description("NULL")]
        Null,

        [Description("ASYNC")]
        Async,

        [Description("EVENT")]
        Event,

        [Description("EXTERN")]
        Extern,

        [Description("FUNCTION")]
        Function,

        [Description("END")]
        End,

        [Description("WHILE")]
        While,

        [Description("DO")]
        Do,

        [Description("LOOP")]
        Loop,

        [Description("BIND")]
        Bind,

        [Description("BREAK")]
        Break,

        [Description("BREAKPOINT")]
        Breakpoint,

        [Description("OR")]
        Or,

        [Description("AND")]
        And,

        [Description("NOT")]
        Not,

        [Description("NEW")]
        New,

        [Description("RETURN")]
        Return,

        [Description("AWAIT")]
        Await,

        [Description("IF")]
        If,

        [Description("THEN")]
        Then,

        [Description("ELSE")]
        Else,

        [Description("TRY")]
        Try,

        [Description("CATCH")]
        Catch,

        [Description("THROW")]
        Throw,

        [Description("EXCEPTION")]
        Exception,

        [Description("String")]
        String,

        [Description("Integer")]
        Integer,

        [Description("Double")]
        Double,

        [Description("TRUE")]
        True,

        [Description("FALSE")]
        False
    }
}
