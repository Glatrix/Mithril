using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using TestLanguage.Language.Manage;

namespace TestLanguage.Language.Language
{
    public class Lexer
    {
        public static Dictionary<string, TokenType> Keywords = new()
        {
            { "var", TokenType.Var },
            { "const", TokenType.Const },
            { "null", TokenType.Null },
            { "function", TokenType.Func },
            { "return", TokenType.Func },
        };

        public static Dictionary<char, char> Escapes = new()
        {
            { '\\', '\\' },
            { '\"', '\"' },
            { '\'', '\'' },
            { 'n', '\n' },
            { 'b', '\b' },
            { 'v', '\v' },
            { 'f', '\f' },
            { 'a', '\a' },
            { 't', '\t' },
            { 'r', '\r' },
            { '0', '\0' },
        };

        public static List<Token> Tokenize(string src)
        {
            List<Token> tokens = new List<Token>();

            char current(bool eat = false)
            {
                char ret = src.FirstOrDefault();
                if (eat) src = src.Substring(1);

                return ret;
            }

            Token HandleIdentifier(string ident)
            {
                if (Keywords.ContainsKey(ident))
                {
                    return new Token(ident, Keywords[ident]);
                }
                return new Token(ident, TokenType.Identifier);
            }

            string readuntil(Predicate<char> comp)
            {
                string read = "";
                while (current() != '\0' && !comp(current())) read += current(true);
                return read;
            }

            string readstring()
            {
                current(true); // eat "
                string text = "";

                while (current() != '\"')
                {
                    if (current() == '\\')
                    {
                        current(true);
                        if (Escapes.ContainsKey(current()))
                        {
                            text += Escapes[current(true)];
                        }
                        else
                        {
                            throw new TokenizerException("Expected Escape Char");
                        }
                    }
                    else
                    {
                        text += current(true);
                    }
                }

                current(true);
                return text;
            }

            // Build Each Token
            while (src.Length > 0)
            {
                if (current() == default(char) || current() == '\0')
                {
                    break;
                }
                else if (current() == '\n') { current(true); continue; }
                else if (current() == ' ') { current(true); continue; }
                else if (current() == '\t') { current(true); continue; }
                else if (current() == '\r') { current(true); continue; }
                else if (current() == ';') { current(true); continue; }

                else if (current() == '+') { tokens.Add(new Token(current(true), TokenType.BinaryOperator)); }
                else if (current() == '-') { tokens.Add(new Token(current(true), TokenType.BinaryOperator)); }
                else if (current() == '*') { tokens.Add(new Token(current(true), TokenType.BinaryOperator)); }
                else if (current() == '/') { tokens.Add(new Token(current(true), TokenType.BinaryOperator)); }
                else if (current() == '%') { tokens.Add(new Token(current(true), TokenType.BinaryOperator)); }
                else if (current() == '^') { tokens.Add(new Token(current(true), TokenType.BinaryOperator)); }


                else if (current() == '=') { tokens.Add(new Token(current(true), TokenType.Equals)); }
                //else if (current() == ';') { tokens.Add(new Token(current(true), TokenType.Semicolon)); }
                else if (current() == ':') { tokens.Add(new Token(current(true), TokenType.Colon)); }

                else if (current() == '.') { tokens.Add(new Token(current(true), TokenType.DOT)); }
                else if (current() == ',') { tokens.Add(new Token(current(true), TokenType.Comma)); }

                else if (current() == '{') { tokens.Add(new Token(current(true), TokenType.OpenBracket)); }
                else if (current() == '}') { tokens.Add(new Token(current(true), TokenType.CloseBracket)); }

                else if (current() == '[') { tokens.Add(new Token(current(true), TokenType.OpenSquareBracket)); }
                else if (current() == ']') { tokens.Add(new Token(current(true), TokenType.CloseSquareBracket)); }

                else if (current() == '(') { tokens.Add(new Token(current(true), TokenType.OpenParen)); }
                else if (current() == ')') { tokens.Add(new Token(current(true), TokenType.CloseParen)); }

                else if (current() == '\"') { tokens.Add(new Token(readstring(), TokenType.StringLiteral)); }

                else if (char.IsLetter(current())) { tokens.Add(HandleIdentifier(readuntil((c) => !char.IsLetterOrDigit(c) && c != '_'))); }

                else if (char.IsDigit(current())) { tokens.Add(new Token(readuntil((c) => !char.IsDigit(c) && c != '.'), TokenType.NumberLiteral)); }

                else { throw new TokenizerException($"Unknown Character: {current()}"); }
            }
            tokens.Add(new Token("eof", TokenType.EOF));
            return tokens;
        }
    }

    public class Token
    {
        public string? value;
        public TokenType type;

        public Token(string? value, TokenType type)
        {
            this.value = value;
            this.type = type;
        }

        public Token(char? value, TokenType type)
        {
            this.value = value.ToString();
            this.type = type;
        }
    }

    public enum TokenType
    {
        Identifier,

        NumberLiteral,
        StringLiteral,

        DOT,                    // .
        Equals,                 // =
        Semicolon,              // ;
        Colon,                  // :
        Comma,                  // ,

        OpenParen,              // )
        CloseParen,             // (

        OpenSquareBracket,      // [
        CloseSquareBracket,     // ]

        OpenBracket,            // {
        CloseBracket,           // }


        BinaryOperator,         // + - * / % ^

        Var,                    // var keyword
        Const,                  // const keyword
        Null,                   // null keyword
        Func,                   // func keyword
        Return,                 // return keyword

        EOF                     // End Of File (Not a char)
    }
}
