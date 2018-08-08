using BaZic.Runtime.BaZic.Code.Lexer.Tokens;
using BaZic.Runtime.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BaZic.Runtime.BaZic.Code.Lexer
{
    /// <summary>
    /// Provides a BaZic code tokenizer.
    /// </summary>
    public sealed class BaZicLexer
    {
        #region Fields & Constants

        private static Dictionary<string, TokenDefinition> tokenDefintions;
        private static Regex notDefinedRegex = new Regex("(\\G\\S+\\s)|\\G\\S+", RegexOptions.Compiled);

        private int _line;
        private int _column;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the code the analyze.
        /// </summary>
        internal string InputCode { get; set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initialize a static instance of the <see cref="BaZicLexer"/> class.
        /// </summary>
        static BaZicLexer()
        {
            // Order is important !
            tokenDefintions = new Dictionary<string, TokenDefinition>()
            {
                { "Comment",        new CommentTokenDefinition() },
                { "NewLine",        new TokenDefinition(TokenType.NewLine, "\n", expectSpaceAfter: false) },
                { "Whitespace",     new TokenDefinition(TokenType.Whitespace, "\r", expectSpaceAfter: false) },
                { "Asterisk",       new TokenDefinition(TokenType.Asterisk, "*", expectSpaceAfter: false) },
                { "Percent",        new TokenDefinition(TokenType.Percent, "%", expectSpaceAfter: false) },
                { "RightBracket",   new TokenDefinition(TokenType.RightBracket, "]", expectSpaceAfter: false) },
                { "RightParenth",   new TokenDefinition(TokenType.RightParenth, ")", expectSpaceAfter: false) },
                { "LeftBracket",    new TokenDefinition(TokenType.LeftBracket, "[", expectSpaceAfter: false) },
                { "LeftParenth",    new TokenDefinition(TokenType.LeftParenth, "(", expectSpaceAfter: false) },
                { "LesserThan",     new TokenDefinition(TokenType.LesserThan, "<", expectSpaceAfter: false) },
                { "GreaterThan",    new TokenDefinition(TokenType.GreaterThan, ">", expectSpaceAfter: false) },
                { "Slash",          new TokenDefinition(TokenType.Slash, "/", expectSpaceAfter: false) },
                { "Dot",            new TokenDefinition(TokenType.Dot, ".", expectSpaceAfter: false) },
                { "And",            new TokenDefinition(TokenType.And, "AND") },
                { "Async",          new TokenDefinition(TokenType.Async, "ASYNC") },
                { "Await",          new TokenDefinition(TokenType.Await, "AWAIT") },
                { "Bind",           new TokenDefinition(TokenType.Bind, "BIND") },
                { "Break",          new TokenDefinition(TokenType.Break, "BREAK") },
                { "Breakpoint",     new TokenDefinition(TokenType.Breakpoint, "BREAKPOINT") },
                { "Catch",          new TokenDefinition(TokenType.Catch, "CATCH") },
                { "Do",             new TokenDefinition(TokenType.Do, "DO") },
                { "Else",           new TokenDefinition(TokenType.Else, "ELSE") },
                { "End",            new TokenDefinition(TokenType.End, "END") },
                { "Event",          new TokenDefinition(TokenType.Event, "EVENT") },
                { "Exception",      new TokenDefinition(TokenType.Exception, "EXCEPTION", expectSpaceAfter: false, keepOriginalValue: true) },
                { "Extern",         new TokenDefinition(TokenType.Extern, "EXTERN") },
                { "False",          new TokenDefinition(TokenType.False, "FALSE") },
                { "Function",       new TokenDefinition(TokenType.Function, "FUNCTION") },
                { "If",             new TokenDefinition(TokenType.If, "IF") },
                { "Loop",           new TokenDefinition(TokenType.Loop, "LOOP") },
                { "New",            new TokenDefinition(TokenType.New, "NEW") },
                { "Not",            new TokenDefinition(TokenType.Not, "NOT") },
                { "Null",           new TokenDefinition(TokenType.Null, "NULL") },
                { "Or",             new TokenDefinition(TokenType.Or, "OR") },
                { "Return",         new TokenDefinition(TokenType.Return, "RETURN") },
                { "Then",           new TokenDefinition(TokenType.Then, "THEN") },
                { "Throw",          new TokenDefinition(TokenType.Throw, "THROW") },
                { "True",           new TokenDefinition(TokenType.True, "TRUE") },
                { "Try",            new TokenDefinition(TokenType.Try, "TRY") },
                { "Variable",       new TokenDefinition(TokenType.Variable, "VARIABLE") },
                { "While",          new TokenDefinition(TokenType.While, "WHILE") },
                { "Double",         new DoubleTokenDefinition() },
                { "Integer",        new IntegerTokenDefinition() },
                { "Equal",          new TokenDefinition(TokenType.Equal, "=", expectSpaceAfter: false) },
                { "Comma",          new TokenDefinition(TokenType.Comma, ",", expectSpaceAfter: false) },
                { "Plus",           new TokenDefinition(TokenType.Plus, "+", expectSpaceAfter: false) },
                { "Minus",          new TokenDefinition(TokenType.Minus, "-", expectSpaceAfter: false) },
                { "Minus2",         new TokenDefinition(TokenType.Minus, "–", expectSpaceAfter: false) },
                { "String",         new StringTokenDefinition() },
                { "Identifier",     new IdentifierTokenDefinition() }
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Try to convert a BaZic code to a <see cref="Token"/> representation.
        /// </summary>
        /// <param name="inputCode">The code to analyze</param>
        /// <param name="keepWhitespaces">Defines whether the whitespaces must be kept in the result of the lexer.</param>
        /// <returns>Returns a list of <see cref="Token"/> that corresponds to the analyzed code.</returns>
        public List<Token> Tokenize(string inputCode, bool keepWhitespaces = false)
        {
            InputCode = inputCode;

            var tokens = new List<Token>();
            var previousTokenType = TokenType.NotDefined;
            _line = 1;
            _column = 1;

            tokens.Add(new Token(TokenType.StartCode, string.Empty, 0, 0, 0, 0));

            var i = 0;
            while (i < InputCode.Length)
            {
                var c = InputCode[i];
                Token token = null;

                switch (char.ToUpperInvariant(c))
                {
                    case '#':
                        token = DetectToken(i, previousTokenType, "Comment");
                        break;

                    case 'A':
                        token = DetectToken(i, previousTokenType, "And", "Async", "Await");
                        break;

                    case 'B':
                        token = DetectToken(i, previousTokenType, "Bind", "Breakpoint", "Break");
                        break;

                    case 'C':
                        token = DetectToken(i, previousTokenType, "Catch");
                        break;

                    case 'D':
                        token = DetectToken(i, previousTokenType, "Do");
                        break;

                    case 'E':
                        token = DetectToken(i, previousTokenType, "Else", "End", "Event", "Exception", "Extern");
                        break;

                    case 'F':
                        token = DetectToken(i, previousTokenType, "False", "Function");
                        break;

                    case 'I':
                        token = DetectToken(i, previousTokenType, "If");
                        break;

                    case 'L':
                        token = DetectToken(i, previousTokenType, "Loop");
                        break;

                    case 'N':
                        token = DetectToken(i, previousTokenType, "New", "Not", "Null");
                        break;

                    case 'O':
                        token = DetectToken(i, previousTokenType, "Or");
                        break;

                    case 'R':
                        token = DetectToken(i, previousTokenType, "Return");
                        break;

                    case 'T':
                        token = DetectToken(i, previousTokenType, "Then", "Throw", "True", "Try");
                        break;

                    case 'V':
                        token = DetectToken(i, previousTokenType, "Variable");
                        break;

                    case 'W':
                        token = DetectToken(i, previousTokenType, "While");
                        break;

                    case '*':
                        token = DetectToken(i, previousTokenType, "Asterisk");
                        break;

                    case '%':
                        token = DetectToken(i, previousTokenType, "Percent");
                        break;

                    case ']':
                        token = DetectToken(i, previousTokenType, "RightBracket");
                        break;

                    case ')':
                        token = DetectToken(i, previousTokenType, "RightParenth");
                        break;

                    case '[':
                        token = DetectToken(i, previousTokenType, "LeftBracket");
                        break;

                    case '(':
                        token = DetectToken(i, previousTokenType, "LeftParenth");
                        break;

                    case '<':
                        token = DetectToken(i, previousTokenType, "LesserThan");
                        break;

                    case '>':
                        token = DetectToken(i, previousTokenType, "GreaterThan");
                        break;

                    case '/':
                        token = DetectToken(i, previousTokenType, "Slash");
                        break;

                    case '.':
                        token = DetectToken(i, previousTokenType, "Dot");
                        break;

                    case '=':
                        token = DetectToken(i, previousTokenType, "Equal");
                        break;

                    case ',':
                        token = DetectToken(i, previousTokenType, "Comma");
                        break;

                    case '+':
                        token = DetectToken(i, previousTokenType, "Double", "Integer", "Plus");
                        break;

                    case '-':
                    case '–':
                        token = DetectToken(i, previousTokenType, "Double", "Integer", "Minus", "Minus2");
                        break;

                    case '\"':
                        token = DetectToken(i, previousTokenType, "String");
                        break;

                    case ' ':
                    case '\r':
                        token = new Token(TokenType.Whitespace, " ", _line, _column, i, 1);
                        break;

                    case '\t':
                        token = new Token(TokenType.Whitespace, "\t", _line, _column, i, 1);
                        break;

                    case '\n':
                        token = new Token(TokenType.NewLine, "\n", _line, _column, i, 1);
                        break;

                    default:
                        if (char.IsDigit(c))
                        {
                            token = DetectToken(i, previousTokenType, "Double", "Integer");
                        }
                        break;
                }

                if (token != null)
                {
                    if (token.TokenType != TokenType.Whitespace || keepWhitespaces)
                    {
                        tokens.Add(new Token(token.TokenType, token.Value, _line, _column, i, token.ParsedLength));
                    }

                    i += token.ParsedLength;
                    previousTokenType = token.TokenType;
                    _column += token.ParsedLength;

                    if (token.TokenType == TokenType.NewLine)
                    {
                        _line++;
                        _column = 0;
                    }
                    if (token.TokenType == TokenType.String)
                    {
                        var stringLines = token.Value.Split('\n');
                        if (stringLines.Length > 1)
                        {
                            _line += stringLines.Length - 1;
                            _column = stringLines.Last().Length + 1; // +1 because of the end quote that is not included in the value.
                        }
                    }
                    continue;
                }

                token = DetectToken(i, previousTokenType, "Identifier");
                if (token != null)
                {
                    tokens.Add(new Token(token.TokenType, token.Value, _line, _column, i, token.ParsedLength));
                    i += token.ParsedLength;
                    previousTokenType = token.TokenType;
                    _column += token.ParsedLength;
                    continue;
                }

                var match = notDefinedRegex.Match(InputCode, i);
                if (match.Success)
                {
                    tokens.Add(new Token(TokenType.NotDefined, match.Value.Trim(), _line, _column, i, match.Value.Length));
                    previousTokenType = TokenType.NotDefined;
                    i += match.Value.Length;
                    _column += match.Value.Length;
                    continue;
                }

                throw new BaZicParserException(_line, _column, i, InputCode.Length - i, L.BaZic.Lexer.UnableGenerateInvalidToken);
            }

            tokens.Add(new Token(TokenType.EndCode, string.Empty, _line, _column, InputCode.Length, 0));

            return tokens;
        }

        /// <summary>
        /// Try to detect a token at a specific part in the code.
        /// </summary>
        /// <param name="startIndex">The position in the code where the detection must start.</param>
        /// <param name="previousTokenType">The previous token type.</param>
        /// <param name="tokenIdentifiers">The list of token definition it must check.</param>
        /// <returns>Returns a token that match the detected token, or return null.</returns>
        private Token DetectToken(int startIndex, TokenType previousTokenType, params string[] tokenIdentifiers)
        {
            for (var i = 0; i < tokenIdentifiers.Length; i++)
            {
                var tokenDefinition = tokenDefintions[tokenIdentifiers[i]];
                var match = tokenDefinition.Match(this, startIndex, previousTokenType);

                if (match.IsMatch)
                {
                    return new Token(match.TokenType, match.Value, _line, _column, startIndex, match.ParsedLength);
                }
            }

            return null;
        }
        #endregion
    }
}