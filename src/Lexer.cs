namespace CryoLang;

class Lexer
{
    private string source;

    private Dictionary<string, TokenType> keywords = new()
    {
        { "fun", TokenType.FUNCTION },
        { "return", TokenType.RETURN },
        { "public", TokenType.PUBLIC },
        { "private", TokenType.PRIVATE },

        { "const", TokenType.CONSTANT },
        { "var", TokenType.VARIABLE },

        // Types
        { "int", TokenType.TYPE },
        { "int8", TokenType.TYPE },
        { "int16", TokenType.TYPE },
        { "int32", TokenType.TYPE },
        { "int64", TokenType.TYPE },

        { "uint8", TokenType.TYPE },
        { "uint16", TokenType.TYPE },
        { "uint32", TokenType.TYPE },
        { "uint64", TokenType.TYPE },

        { "float32", TokenType.TYPE },
        { "float64", TokenType.TYPE },

        { "char", TokenType.TYPE},
    };

    public Lexer(string code){
        this.source = code;
    }

    public List<Token> Tokenize()
    {
        List<Token> tokens = new List<Token>();

        int i = 0;

        while (i < source.Length)
        {
            char ch = source[i];

            // Skipping whitespace
            if (char.IsWhiteSpace(ch))
            {
                i++;
                continue;
            }

            // Identifiers / Keywords / Types
            if (char.IsLetter(ch))
            {
                string value = "";

                while (i < source.Length && char.IsLetterOrDigit(source[i]))
                {
                    value += source[i];
                    i++;
                }

                // Check keyword map
                if (keywords.TryGetValue(value, out TokenType type))
                {
                    tokens.Add(new Token(type, value));
                }
                else
                {
                    tokens.Add(new Token(TokenType.IDENT, value));
                }

                continue;
            }

            // Numbers
            if (char.IsDigit(ch))
            {
                string value = "";

                while (i < source.Length && char.IsDigit(source[i]))
                {
                    value += source[i];
                    i++;
                }

                tokens.Add(new Token(TokenType.NUMBER, value));
                continue;
            }

            // Multi-character tokens (->)
            if (ch == '-' && i + 1 < source.Length && source[i + 1] == '>')
            {
                tokens.Add(new Token(TokenType.ARROW, "->"));
                i += 2;
                continue;
            }

            // Single-character tokens
            switch (ch)
            {
                case '(': tokens.Add(new Token(TokenType.LPAREN, "(")); break;
                case ')': tokens.Add(new Token(TokenType.RPAREN, ")")); break;
                case '{': tokens.Add(new Token(TokenType.LBRACE, "{")); break;
                case '}': tokens.Add(new Token(TokenType.RBRACE, "}")); break;
                case ',': tokens.Add(new Token(TokenType.COMMA, ",")); break;
                case ';': tokens.Add(new Token(TokenType.SEMICOLON, ";")); break;
                case '+': tokens.Add(new Token(TokenType.PLUS, "+")); break;
                case '-': tokens.Add(new Token(TokenType.MINUS, "-")); break;
                case '*': tokens.Add(new Token(TokenType.STAR, "*")); break;
                case '/': tokens.Add(new Token(TokenType.SLASH, "/")); break;
                case '%': tokens.Add(new Token(TokenType.MOD, "%")); break;
                case '=': tokens.Add(new Token(TokenType.EQUALS, "=")); break;

                default:
                    throw new Exception($"Unexpected character: {ch}");
            }

            i++;
        }

        tokens.Add(new Token());
        return tokens;
    }
}