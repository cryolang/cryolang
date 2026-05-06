namespace CryoLang;

class Lexer
{
    private readonly string source;

    private readonly Dictionary<string, TokenType> keywords = new()
    {
        { "cryolang", TokenType.CRYOLANG },
        { "import", TokenType.IMPORT },
        { "macro", TokenType.MACRO },
        { "fun", TokenType.FUNCTION },
        { "return", TokenType.RETURN },
        { "public", TokenType.PUBLIC },
        { "private", TokenType.PRIVATE },
        { "if", TokenType.IF },
        { "else", TokenType.ELSE },
        { "while", TokenType.WHILE },

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
        { "void", TokenType.TYPE},
    };

    public Lexer(string code){
        source = code;
    }

    public List<Token> Tokenize()
    {
        List<Token> tokens = [];

        int i = 0;
        int line = 1;
        int column = 1;

        void Add(TokenType type, string value, int tokenColumn) => tokens.Add(new Token(type, value, line, tokenColumn));
        char Peek(int offset = 0) => i + offset < source.Length ? source[i + offset] : '\0';
        void Step()
        {
            if (Peek() == '\n')
            {
                line++;
                column = 1;
            }
            else
            {
                column++;
            }
            i++;
        }

        while (i < source.Length)
        {
            char ch = Peek();

            if (ch == '\r')
            {
                Step();
                continue;
            }

            if (ch == '\n')
            {
                Add(TokenType.NEWLINE, "\\n", column);
                Step();
                continue;
            }

            // Skipping non-newline whitespace
            if (char.IsWhiteSpace(ch))
            {
                Step();
                continue;
            }

            // Line comments
            if (ch == '/' && Peek(1) == '/')
            {
                while (i < source.Length && Peek() != '\n')
                    Step();
                continue;
            }

            // Paths for local imports, e.g. ./local.cryo or ../foo.cryo.
            if (ch == '.' && (Peek(1) == '/' || Peek(1) == '.'))
            {
                int tokenColumn = column;
                string value = "";
                while (i < source.Length && !char.IsWhiteSpace(Peek()) && Peek() != ';')
                {
                    value += Peek();
                    Step();
                }
                Add(TokenType.PATH, value, tokenColumn);
                continue;
            }

            // Identifiers / Keywords / Types
            if (char.IsLetter(ch) || ch == '_')
            {
                int tokenColumn = column;
                string value = "";

                while (i < source.Length && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
                {
                    value += Peek();
                    Step();
                }

                if (keywords.TryGetValue(value, out TokenType type))
                    Add(type, value, tokenColumn);
                else
                    Add(TokenType.IDENT, value, tokenColumn);

                continue;
            }

            // Numbers and language versions (1.0.0).
            if (char.IsDigit(ch))
            {
                int tokenColumn = column;
                string value = "";
                bool hasDot = false;

                while (i < source.Length && (char.IsDigit(Peek()) || Peek() == '.'))
                {
                    if (Peek() == '.')
                        hasDot = true;

                    value += Peek();
                    Step();
                }

                Add(hasDot ? TokenType.VERSION : TokenType.NUMBER, value, tokenColumn);
                continue;
            }

            int col = column;

            // Multi-character tokens
            if (ch == '-' && Peek(1) == '>') { Add(TokenType.ARROW, "->", col); Step(); Step(); continue; }
            if (ch == '+' && Peek(1) == '+') { Add(TokenType.PLUS_PLUS, "++", col); Step(); Step(); continue; }
            if (ch == '=' && Peek(1) == '=') { Add(TokenType.EQUAL_EQUAL, "==", col); Step(); Step(); continue; }
            if (ch == '!' && Peek(1) == '=') { Add(TokenType.BANG_EQUAL, "!=", col); Step(); Step(); continue; }
            if (ch == '<' && Peek(1) == '=') { Add(TokenType.LESS_EQUAL, "<=", col); Step(); Step(); continue; }
            if (ch == '>' && Peek(1) == '=') { Add(TokenType.GREATER_EQUAL, ">=", col); Step(); Step(); continue; }

            // Single-character tokens
            switch (ch)
            {
                case '(': Add(TokenType.LPAREN, "(", col); break;
                case ')': Add(TokenType.RPAREN, ")", col); break;
                case '{': Add(TokenType.LBRACE, "{", col); break;
                case '}': Add(TokenType.RBRACE, "}", col); break;
                case ',': Add(TokenType.COMMA, ",", col); break;
                case ';': Add(TokenType.SEMICOLON, ";", col); break;
                case '+': Add(TokenType.PLUS, "+", col); break;
                case '-': Add(TokenType.MINUS, "-", col); break;
                case '*': Add(TokenType.STAR, "*", col); break;
                case '/': Add(TokenType.SLASH, "/", col); break;
                case '%': Add(TokenType.MOD, "%", col); break;
                case '=': Add(TokenType.EQUALS, "=", col); break;
                case '<': Add(TokenType.LESS, "<", col); break;
                case '>': Add(TokenType.GREATER, ">", col); break;
                case '?': Add(TokenType.QUESTION, "?", col); break;
                case ':': Add(TokenType.COLON, ":", col); break;
                default:
                    throw new Exception($"Unexpected character '{ch}' at {line}:{column}");
            }

            Step();
        }

        tokens.Add(new Token(TokenType.EOF, "", line, column));
        return tokens;
    }
}
